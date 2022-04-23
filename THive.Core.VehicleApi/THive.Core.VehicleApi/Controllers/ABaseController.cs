using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using THive.Core.DeviceApi.Models;
using THive.Core.DeviceApi.MongoDb;

namespace THive.Core.DeviceApi.Controllers;

public abstract class ABaseController<TModel, TPrimaryKey> : ControllerBase 
    where TPrimaryKey : IEquatable<TPrimaryKey>
    where TModel : IApiModel<TPrimaryKey>
{
    protected readonly ILogger<ABaseController<TModel, TPrimaryKey>> Logger;
    protected readonly IGenericMongoRepository<TModel, TPrimaryKey> Repository;


    protected ABaseController(ILogger<ABaseController<TModel, TPrimaryKey>> logger, IGenericMongoRepository<TModel, TPrimaryKey> repository)
    {
        Logger = logger;
        Repository = repository;
    }

    /// <summary>
    /// Get all entries from the current DataBase.
    /// </summary>
    /// <returns>A collection of <see cref="TModel"/></returns>
    /// <response code="200">Returns all database entries.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<>))] //IEnumerable<TModel>
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public virtual async Task<IActionResult> Get()
    {
        try
        {
            var result = await Repository.GetAllAsync<TModel, TPrimaryKey>(_ => true);
            return Ok(result);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error calling Get for all {entity}", typeof(TModel).Name);
            return Problem($"Failed to retrieve {typeof(TModel).Name}");
        }
    }
}