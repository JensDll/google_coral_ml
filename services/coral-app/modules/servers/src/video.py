import zmq
import zmq.asyncio
import logging
import cv2
import numpy as np
import functools
from modules import core, inference
from typing import Union


async def load_model(
    load_model_peer: zmq.asyncio.Socket, args: inference.detection.models.ModelArgs
) -> Union[core.typedef.RunInference, None]:
    json: core.typedef.LoadModelResult = await load_model_peer.recv_json()
    logging.info("[VIDEO] Received Interpreter")
    args["labels"] = core.coral.load_labels(json["labelPath"])
    interpreter = core.coral.load_interpreter(json["modelPath"])
    model_name = json["record"]["modelFileName"]
    logging.info("[VIDEO] Sending Response ...")
    if hasattr(inference.detection.models, model_name):
        core.zutils.send_normalized_json(load_model_peer)
        return functools.partial(
            getattr(inference.detection.models, model_name),
            interpreter=interpreter,
            args=args,
        )
    else:
        core.zutils.send_normalized_json(
            load_model_peer, errors=["This model is not supported"]
        )
    return None


async def update_settings(
    socket: zmq.asyncio.Socket, args: inference.detection.models.ModelArgs
):
    settings: core.typedef.ModelSettings = await socket.recv_json()
    args["topK"] = settings["topK"]
    args["scoreThreshold"] = settings["threshold"]
    return socket.send(b"")


async def start(reset_peer: zmq.asyncio.Socket, load_model_peer: zmq.asyncio.Socket):
    update_settings_addr = f"tcp://*:{core.CONFIG.PORTS.VIDEO_UPDATE_SETTINGS}"
    update_settings_socket: zmq.asyncio.Socket = core.CONFIG.ZMQ.CONTEXT.socket(zmq.REP)
    update_settings_socket.bind(update_settings_addr)
    logging.info(f"[VIDEO] (Update Settings) Bind to '{update_settings_addr}'")

    poller = zmq.asyncio.Poller()
    poller.register(reset_peer, zmq.POLLIN)
    poller.register(load_model_peer, zmq.POLLIN)
    poller.register(update_settings_socket, zmq.POLLIN)

    cap = cv2.VideoCapture(0)
    cap_props: core.typedef.CapProps = {
        "frameWidth": int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)),
        "frameHeight": int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT)),
        "fps": int(cap.get(cv2.CAP_PROP_FPS)),
    }
    process = core.stream.start_stream(cap_props)

    run_inference: Union[core.typedef.RunInference, None] = None
    args: inference.detection.models.ModelArgs = {
        "topK": 1,
        "scoreThreshold": 0.1,
        "labels": dict(),
    }

    # Signal video server ready
    await load_model_peer.send(b"")

    while True:
        try:
            items = dict(await poller.poll(0))
        except:
            break

        if reset_peer in items:
            await reset_peer.recv()
            run_inference = None
            logging.info("[VIDEO] Reset")
            await reset_peer.send(b"")

        if load_model_peer in items:
            run_inference = await load_model(load_model_peer, args)

        if update_settings_socket in items:
            await update_settings(update_settings_socket, args)

        image: np.ndarray = cap.read()[1]
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

        if run_inference is not None:
            run_inference(image=image)

        try:
            process.stdin.write(image.tobytes())
        except:
            process = next(core.stream.restart_stream(cap_props))
