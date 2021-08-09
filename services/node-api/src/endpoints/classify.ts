import { Listener, MessageEnvelope } from './types'
import zmq from 'zeromq'

type Response = {
  probabilities: number[]
  classes: string[]
  inferenceTime: number
}

type RequestData = {
  image: Buffer
  format: string
}

export const classify =
  (client: zmq.Request) =>
  async (
    { image, format }: RequestData,
    callback: Listener<MessageEnvelope<Response>>
  ) => {
    await client.send([image, format])
    const [result] = await client.receive()
    const response: MessageEnvelope<Response> = JSON.parse(result.toString())
    callback(response)
  }
