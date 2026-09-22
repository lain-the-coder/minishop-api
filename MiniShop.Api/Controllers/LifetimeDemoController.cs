using Microsoft.AspNetCore.Mvc;
using MiniShop.Api.Demo;

namespace MyApp.Namespace
{
    [Route("api/demo/lifetimes")]
    [ApiController]
    public class LifetimeDemoController(
        SingletonOp singletonOp1,
        SingletonOp singletonOp2,
        TransientOp transientOp1,
        TransientOp transientOp2,
        ScopedOp scopedOp1,
        ScopedOp scopedOp2
        ) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetLifeTimes()
        {
            var singletonData = new
            {
                First = singletonOp1.ID,
                Second = singletonOp2.ID,
                IsSame = singletonOp1.ID == singletonOp2.ID
            };
            var transientData = new
            {
                First = transientOp1.ID,
                Second = transientOp2.ID,
                IsSame = transientOp1.ID == transientOp2.ID
            };
            var scopedData = new
            {
                First = scopedOp1.ID,
                Second = scopedOp2.ID,
                IsSame = scopedOp1.ID == scopedOp2.ID
            };
            var payload = new
            {
                Singleton = singletonData,
                Transient = transientData,
                Scoped = scopedData
            };
            return Ok(payload);
        }
    }
}

/*
{
  "singleton": {
    "first": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "second": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "isSame": true
  },
  "scoped": {
    "first": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "second": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "isSame": true
  },
  "transient": {
    "first": "d3b07384-d113-46fb-9b93-5561b369ec2a",
    "second": "c9a646d3-9c61-4cb7-897b-91d4e0e5672a",
    "isSame": false
  }
}
*/