﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Application.Data.DataTransfer;
using Application.Data.DataTransfer.Record;
using Application.Data.Repositories;
using Application.Mapping.Record;
using Contracts;
using Contracts.Request;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace ModelAPI.Controllers
{
    [ApiController]
    public class RecordController : ControllerBase
    {
        private readonly IRecordRepository _recordRepository;
        private readonly IRecordRequestMapper _requestMapper;

        public RecordController(IRecordRepository recordRepository, IRecordRequestMapper requestMapper)
        {
            _recordRepository = recordRepository;
            _requestMapper = requestMapper;
        }

        [HttpGet(ApiRoutes.Record.GetWithRecordTypeId)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PaginationEnvelope<RecordGetAll>), StatusCodes.Status200OK)]
        public async Task<IActionResult>
            GetWithRecordTypeId([FromQuery] PaginationRequestDto paginationRequestDto, int recordTypeId)
        {
            var pagination = _requestMapper.MapPaginationRequest(paginationRequestDto);

            var envelope = await _recordRepository.GetWithRecordTypeIdAsync(pagination, recordTypeId);

            return Ok(envelope);
        }

        [HttpGet(ApiRoutes.Record.GetLoaded)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RecordGetLoaded), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLoaded()
        {
            var loadedRecord = await _recordRepository.GetLoadedAsync();

            if (loadedRecord == null)
            {
                return NotFound();
            }

            return Ok(loadedRecord);
        }

        [HttpGet(ApiRoutes.Record.GetById)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RecordGetById), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _recordRepository.GetByIdAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            return Ok(record);
        }

        [HttpGet(ApiRoutes.Record.Download)]
        [Produces("application/zip", "application/json")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(int id)
        {
            
            byte[] zipContent = await _recordRepository.DownloadAsync(id);

            if (zipContent == null)
            {
                return NotFound();
            }

            return File(zipContent, "application/zip", "record.zip");
        }

        [HttpPost(ApiRoutes.Record.Upload)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Upload([FromForm] int recordTypeId, IFormFile model, IFormFile label)
        {
            var createData = await _requestMapper.MapCreateRequestAsync(recordTypeId, model, label);
            
            int id = await _recordRepository.CreateAsync(createData);

            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPut(ApiRoutes.Record.SetLoaded)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SetLoaded(int id)
        {
            await _recordRepository.SetLoadedAsync(id);

            return NoContent();
        }

        [HttpPut(ApiRoutes.Record.Unload)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Unload()
        {
            await _recordRepository.UnloadAsync();

            return NoContent();
        }

        [HttpDelete(ApiRoutes.Record.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            await _recordRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
