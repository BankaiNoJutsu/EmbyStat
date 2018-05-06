﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EmbyStat.Controllers.ViewModels.Movie;
using EmbyStat.Controllers.ViewModels.Stat;
using EmbyStat.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmbyStat.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class MovieController : Controller
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        [Route("getcollections")]
        public IActionResult GetCollections()
        {
            var result = _movieService.GetMovieCollections();
            return Ok(Mapper.Map<IList<CollectionViewModel>>(result));
        }

        [HttpPost]
        [Route("getgeneralstats")]
        public IActionResult GetGeneralStats([FromBody]List<string> collectionIds)
        {
            var result = _movieService.GetGeneralStatsForCollections(collectionIds);
            var convert = Mapper.Map<MovieStatsViewModel>(result);
            return Ok(convert);
        }

        [HttpPost]
        [Route("getpersonstats")]
        public async Task<IActionResult> GetPersonStats([FromBody]List<string> collectionIds)
        {
            var result = await _movieService.GetPeopleStatsForCollections(collectionIds);
            return Ok(Mapper.Map<MoviePersonStatsViewModel>(result));
        }

        [HttpPost]
        [Route("getduplicates")]
        public IActionResult GetDuplicates([FromBody] List<string> collectionIds)
        {
            var result = _movieService.GetDuplicates(collectionIds);
            return Ok(Mapper.Map<IList<MovieDuplicateViewModel>>(result));
        }

        [HttpGet]
        [Route("getchartdata")]
        public IActionResult GetChartData()
        {
            var result = "[{\"name\": \"Germany\", \"value\": 40632},{\"name\": \"France\", \"value\": 36745 }, { \"name\": \"United Kingdom\",\"value\": 36240},{ \"name\": \"Spain\",\"value\": 33000}, {\"name\": \"Uganda\",\"value\": 19294}, {\"name\": \"Bonaire, Sint Eustatius and Saba\",\"value\": 50184},{\"name\": \"India\",\"value\": 20303}]";

            return Ok(result);
        }
    }
}
