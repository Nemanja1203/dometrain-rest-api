namespace Movies.Api.Sdk;

// Note: This file is copied from Movies.Api
public static class ApiEndpoints
{
    private const string ApiBase = "/api"; // Note: Slash has beed added in fron of 'api'

    public static class Movies
    {
        private const string Base = $"{ApiBase}/movies";

        public const string Create = Base;
        //public const string Get = $"{Base}/{{id}}";
        public const string Get = $"{Base}/{{idOrSlug}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id}}";// Note: ':guid' constraint has been removed
        public const string Delete = $"{Base}/{{id}}";

        public const string Rate = $"{Base}/{{id}}/ratings";
        public const string DeleteRating = $"{Base}/{{id}}/ratings";
    }

    public static class Ratings
    {
        private const string Base = $"{ApiBase}/ratings";

        public const string GetUserRatings = $"{Base}/me";
    }
}
