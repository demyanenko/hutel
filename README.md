Human Telemetry
===

To run the project, grab the latest [.NET Core](https://www.microsoft.com/net/core) and run
```
$ dotnet restore
$ dotnet run
```
in the /server folder.

To run the tests, execute
```
$ dotnet test
```
in the /server.test folder.


## Usage
The server acts as a storage for data points, which are validated against the corresponding tags from the `tags.json` file. Each point must contain a tag id, date and all of the fields required by its tag. There are six field types supported: int, float, string, date, time, enum.

Examples of the points (rename `tags.json.example` to `tags.json` for successful validation):
```
{
    'tagId': 'pushups',
    'date': '2016-12-11',
    'sets': 5,
    'total': 130
}
```
```
{
    'tagId': 'enoughsleep',
    'date': '2016-12-12',
    'value': 'no'
}
```
```
{
    'tagId': 'vacation',
    'date': '2016-12-17',
    'location': 'Mars',
    'endDate': '2016-12-31'
}
```
```
{
    'tagId': 'run',
    'date': '2017-01-01',
    'km': 42.0,
    'time': '02:34:11'
}
```

## Methods
* POST `/api/points` with point in the body adds a new point to the storage, validating it against corresponding tag
* GET `/api/points` returns the entire storage contents
* GET `/api/points?startDate=yyyy-mm-dd` returns all points that were added on or after specified date
* PUT `/api/points` overwrites the entire storage, the body format is the same as result of the previous method; all points are validated against tags, if any single one doesn't pass validation, storage is not modified
* PUT `/api/point/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` overwrites the point with specified id; the new point must include the same tag id as the old one
* GET `/api/tags` returns the entire tags config contents
* PUT `/api/tags` overwrites the entire tags config, the body format is the same as result of the previous method; all existing points are validated against the new tags, if any given one doesn't pass validation, tags config is not modified
>>>>>>> 67ce7a2d0180e90ba0079b998705a666b345cac0
