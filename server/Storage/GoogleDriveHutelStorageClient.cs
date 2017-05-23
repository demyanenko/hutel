using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using hutel.Logic;
using Microsoft.Extensions.Caching.Memory;

using GoogleFile = Google.Apis.Drive.v2.Data.File;
using ParentReference = Google.Apis.Drive.v2.Data.ParentReference;

namespace hutel.Storage
{
    public class GoogleDriveHutelStorageClient : IHutelStorageClient
    {
        private const string ApplicationName = "Human Telemetry";
        private const string RootFolderName = "Hutel";
        private const string PointsFileName = "storage.json";
        private const string TagsFileName = "tags.json";
        private const string FolderMimeType = "application/vnd.google-apps.folder";
        private const string JsonMimeType = "application/octet-stream";
        private readonly string _userId;
        private bool _initialized;
        private DriveService _driveService;
        private string _rootFolderId;
        private string _pointsFileId;
        private string _tagsFileId;
        
        public GoogleDriveHutelStorageClient(string userId)
        {
            _userId = userId;
            _initialized = false;
        }

        public async Task<string> ReadPointsAsStringAsync()
        {
            await InitAsync();
            Console.WriteLine("ReadPointsAsStringAsync");
            return await ReadFileAsStringAsync(_pointsFileId);
        }

        public async Task WritePointsAsStringAsync(string data)
        {
            await InitAsync();
            Console.WriteLine("WritePointsAsStringAsync");
            await WriteFileAsStringAsync(_pointsFileId, data);
        }

        public async Task<string> ReadTagsAsStringAsync()
        {
            await InitAsync();
            Console.WriteLine("ReadTagsAsStringAsync");
            return await ReadFileAsStringAsync(_tagsFileId);
        }

        public async Task WriteTagsAsStringAsync(string data)
        {
            await InitAsync();
            Console.WriteLine("WriteTagsAsStringAsync");
            await WriteFileAsStringAsync(_tagsFileId, data);
        }

        private async Task InitAsync()
        {
            if (_initialized)
            {
                return;
            }
            var HttpClientInitializer = new GoogleHttpClientInitializer(_userId);
            _driveService = new DriveService(
                new DriveService.Initializer
                {
                    HttpClientInitializer = HttpClientInitializer,
                    ApplicationName = ApplicationName
                }
            );

            _rootFolderId = await GetOrCreateFile(FolderMimeType, RootFolderName, null);
            _pointsFileId = await GetOrCreateFile(null, PointsFileName, _rootFolderId);
            _tagsFileId = await GetOrCreateFile(null, TagsFileName, _rootFolderId);
            _initialized = true;
        }

        private async Task<string> GetOrCreateFile(string mimeType, string name, string parent)
        {
            var listRequest = _driveService.Files.List();
            var parentId = parent != null ? parent : "root";
            var mimeQuery = mimeType != null ? $"mimeType = '{mimeType}' and " : "";
            listRequest.Q = mimeQuery + $"title = '{name}' and '{parentId}' in parents";
            listRequest.Spaces = "drive";
            var fileList = await listRequest.ExecuteAsync();
            var validFiles = fileList.Items.Where(file => file.Labels.Trashed != true).ToList();
            if (validFiles.Count > 0)
            {
                return fileList.Items[0].Id;
            }
            else
            {
                var fileMetadata = new GoogleFile
                {
                    Title = name,
                    MimeType = mimeType,
                    Parents = new List<ParentReference>
                        {
                            parent != null
                                ? new ParentReference{ Id = parentId }
                                : new ParentReference{ IsRoot = true }
                        }
                };
                var request = _driveService.Files.Insert(fileMetadata);
                request.Fields = "id";
                var file = await request.ExecuteAsync();
                return file.Id;
            }
        }

        private async Task<string> ReadFileAsStringAsync(string fileId)
        {
            Console.WriteLine("ReadFileAsStringAsync");
            var downloadRequest = _driveService.Files.Get(fileId);
            var stream = new MemoryStream();
            var progress = await downloadRequest.DownloadAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(stream);
            var contents = await streamReader.ReadToEndAsync();
            return contents;
        }

        private async Task WriteFileAsStringAsync(string fileId, string data)
        {
            Console.WriteLine("WriteFileAsStringAsync");
            
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(data);
            streamWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var file = new GoogleFile();
            var updateRequest = _driveService.Files.Update(file, fileId, stream, JsonMimeType);
            var progress = await updateRequest.UploadAsync();
        }
    }
}