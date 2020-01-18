using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AssistantAssignment.WebApp.Repositories;

namespace AssistantAssignment.WebApp.Controllers
{
    public class ListTaskController
    {
        private readonly IGeneticAlgorithmTaskRepository _repository;

        public ListTaskController(IGeneticAlgorithmTaskRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("/task")]
        public JsonResult ListAll()
        {
            return new JsonResult(_repository.All());
        }
    }
}
