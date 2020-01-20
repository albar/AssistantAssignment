using Microsoft.AspNetCore.Mvc;
using AssistantAssignment.Web.Api.Repositories;

namespace AssistantAssignment.Web.Api.Controllers
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
