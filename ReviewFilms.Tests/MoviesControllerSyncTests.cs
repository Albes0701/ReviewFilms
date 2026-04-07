using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using ReviewFilms.Api.Controllers;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class MoviesControllerSyncTests
{
    [Fact]
    public void MoviesController_write_endpoints_use_permission_based_authorization()
    {
        AssertHttpMethodHasPermission(typeof(MoviesController), "CreateMovie", typeof(HttpPostAttribute), null, "movies:create");
        AssertHttpMethodHasPermission(typeof(MoviesController), "UpdateMovie", typeof(HttpPutAttribute), "{id:guid}", "movies:update");
        AssertHttpMethodHasPermission(typeof(MoviesController), "SyncGenres", typeof(HttpPostAttribute), "sync-genres", "genres:sync");
        AssertHttpMethodHasPermission(typeof(MoviesController), "ImportSingle", typeof(HttpPostAttribute), "import/single/{tmdbId:int}", "movies:import");
        AssertHttpMethodHasPermission(typeof(MoviesController), "ImportBulk", typeof(HttpPostAttribute), "import/bulk", "movies:import");
    }

    [Fact]
    public void Read_only_lookup_controllers_expose_expected_routes()
    {
        var genresControllerType = typeof(MoviesController).Assembly.GetType("ReviewFilms.Api.Controllers.GenresController");
        var personsControllerType = typeof(MoviesController).Assembly.GetType("ReviewFilms.Api.Controllers.PersonsController");

        Assert.NotNull(genresControllerType);
        Assert.NotNull(personsControllerType);
        Assert.Equal("api/genres", genresControllerType!.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal("api/persons", personsControllerType!.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    [Fact]
    public void GetMovies_exposes_person_id_as_query_parameter()
    {
        var method = typeof(MoviesController).GetMethod("GetMovies");

        Assert.NotNull(method);

        var parameter = Assert.Single(
            method!.GetParameters(),
            item => item.Name == "personId");

        Assert.Equal(typeof(Guid?), parameter.ParameterType);
        Assert.NotNull(parameter.GetCustomAttribute<FromQueryAttribute>());
    }

    private static void AssertHttpMethodHasPermission(
        Type controllerType,
        string methodName,
        Type httpMethodAttributeType,
        string? expectedRoute,
        string expectedPermission)
    {
        var method = controllerType.GetMethod(methodName);
        var permissionAttributeType = controllerType.Assembly.GetType("ReviewFilms.Api.Security.HasPermissionAttribute");

        Assert.NotNull(method);
        Assert.NotNull(permissionAttributeType);

        var authorizeAttribute = method!.GetCustomAttributes(permissionAttributeType!, inherit: true)
            .OfType<AuthorizeAttribute>()
            .SingleOrDefault();
        var httpMethodAttribute = method.GetCustomAttributes(httpMethodAttributeType, inherit: true)
            .Cast<HttpMethodAttribute>()
            .SingleOrDefault();

        Assert.NotNull(authorizeAttribute);
        Assert.NotNull(httpMethodAttribute);
        Assert.Equal(expectedRoute, httpMethodAttribute!.Template);
        Assert.Equal(
            expectedPermission,
            permissionAttributeType!.GetProperty("Permission")!.GetValue(authorizeAttribute));
        Assert.Equal($"Permission:{expectedPermission}", authorizeAttribute.Policy);
    }
}
