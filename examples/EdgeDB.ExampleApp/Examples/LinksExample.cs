using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class LinksExample : IExample
    {
        public ILogger? Logger { get; set; }

        public class Person
        {
            [EdgeDBProperty("name")]
            public string? Name { get; set; }

            [EdgeDBProperty("email")]
            public string? Email { get; set; }
        }

        public class Movie
        {
            [EdgeDBProperty("title")]
            public string? Title { get; set; }

            [EdgeDBProperty("year")]
            public int Year { get; set; }

            [EdgeDBProperty("director")]
            public Person? Director { get; set; }

            [EdgeDBProperty("actors")]
            public Person[]? Actors { get; set; }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            // create a new movie
            var createMovieQuery = "with" + 
                                   "  cnolan:= (insert Person { name:= \"Christopher Nolan\", email:= \"cnolan@imdb.com\"}" +
                                   "    unless conflict on.email else (select Person))," + 
                                   "  leonardo:= (insert Person { name:= \"Leonardo DiCaprio\", email:= \"ldicaprio@imdb.com\" }" +
                                   "    unless conflict on.email else (select Person))," +
                                   "  joseph:= (insert Person { name:= \"Joseph Gordon-Levitt\", email:= \"jgordonlevitt @imdb.com\"}" +
                                   "    unless conflict on.email else (select Person))" +
                                   "insert Movie { title:= \"Inception\", year:= 2010, director:= cnolan, actors:= { leonardo, joseph} }" +
                                   "  unless conflict on.title else (select Movie);";

            var selectMovieQuery = "select Movie {title, year, director: {name, email}, actors: {name, email}} filter .title = 'Inception'";

            await client.ExecuteAsync(createMovieQuery).ConfigureAwait(false);


            // select it
            var movie = await client.QueryRequiredSingleAsync<Movie>(selectMovieQuery).ConfigureAwait(false);

            Logger?.LogInformation("Movie: {@Movie}", movie);
        }
    }
}
