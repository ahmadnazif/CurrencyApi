﻿using Microsoft.AspNetCore.Mvc;

namespace CurrencyApi.Controllers;


[ApiExplorerSettings(IgnoreApi = true)]
[Route("/")]
[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet]
    public ActionResult Redirect() => Redirect("swagger");
}