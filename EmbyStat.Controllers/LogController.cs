﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AutoMapper;
using EmbyStat.Controllers.ViewModels.Logs;
using EmbyStat.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmbyStat.Controllers
{
    [Route("api/[controller]")]
    public class LogController : Controller
    {
        private readonly ILogService _logService;
        private readonly IMapper _mapper;

        public LogController(ILogService logService, IMapper mapper)
        {
            _logService = logService;
            _mapper = mapper;
        }

        [HttpGet]
        [Produces("application/json")]
        [Route("list")]
        public IActionResult GetLogFileLIst()
        {
            var files = _logService.GetLogFileList();
            return Ok(_mapper.Map<IList<LogFileViewModel>>(files));
        }

        [HttpGet]
        [Route("download/{fileName}")]
        public FileResult GetZipFile(string fileName, bool anonymous)
        {
            try
            {
                var stream = _logService.GetLogStream(fileName, anonymous);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}
