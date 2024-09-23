using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;

using var settings = new ElasticsearchClientSettings(new Uri("https://elasticsearch.aonprd.com"));
var client = new ElasticsearchClient(settings);

var request = new SearchRequest<dynamic>("aon")
{
    From = 0,
    Size = 10,
    Query = new BoolQuery
    {
        Filter =
        [
            new TermQuery("category"!) { Value = "creature" },
            new TermsQuery() {Field = "name"!, Term = new TermsQueryField(["mitflit", "slurk"]) }
        ],
        MustNot = [
            new ExistsQuery() { Field = "remaster_id"! },
            new ExistsQuery() { Field = "item_child_id"! },
            new TermQuery("exclude_from_search"!) { Value = true }
        ]
    },
    Source = new SourceConfig(new SourceFilter
    {
        Includes = Fields.FromStrings(["id", "name", "remaster_id", "item_child_id", "exclude_from_search"])
    })
};

var response = await client.SearchAsync<dynamic>(request);

if (response.IsValidResponse)
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    var json = JsonSerializer.Serialize(response.Documents, options);
    Console.WriteLine(json);
}
else
{
    Console.WriteLine("Error");
}