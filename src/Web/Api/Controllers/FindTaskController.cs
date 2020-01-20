using Microsoft.AspNetCore.Mvc;
using AssistantAssignment.Web.Api.Repositories;

namespace AssistantAssignment.Web.Api.Controllers
{
    public class FindTaskController
    {
        private readonly IGeneticAlgorithmTaskRepository _repository;

        public FindTaskController(IGeneticAlgorithmTaskRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("/task/{id}")]
        public JsonResult Find(string id)
        {
            return new JsonResult(_repository.Find(id));
        }
    }
}
