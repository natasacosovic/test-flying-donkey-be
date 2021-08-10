using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace task_test_flying_donkey.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class FileController : ControllerBase
  {
    private readonly IConfiguration Configuration;
    public FileController(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    [HttpGet("types")]
    public IEnumerable<string> GetFileTypes()
    {
      List<string> fileTypes = Configuration.GetSection($"{"FileFormat"}").Get<List<string>>();
      return fileTypes;
    }

    [HttpGet("size")]
    public int GetFileSize()
    {
      int size = Int32.Parse(Configuration["FileSize"]);
      return size;
    }

    [HttpGet]
    public IEnumerable<FileMetadata> Get()
    {
      List<FileMetadata> filesInfo = new List<FileMetadata>();
      var uploads = Path.Combine(Configuration["UploadFolder"]);
      if (Directory.Exists(uploads))
      {
        string[] fileEntries = Directory.GetFiles(uploads);
        foreach (string fileName in fileEntries)
        {
          FileInfo fi = new FileInfo(fileName);
          FileMetadata newFile = new FileMetadata() {
            Name = fi.Name,
            Date = fi.CreationTime.ToString(),
            Size = fi.Length };

          filesInfo.Add(newFile);
        }
      }
      else
      {
        Console.WriteLine("{0} is not a valid directory.", uploads);
      }
      return filesInfo;
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
      var memory = new MemoryStream();
      var filePath = Path.Combine(Configuration["UploadFolder"]) + "\\" + fileName;
      using (var stream = new FileStream(filePath, FileMode.Open))
      {
        await stream.CopyToAsync(memory);
      }
      memory.Position = 0;
      return File(memory, GetContentType(filePath), fileName);
    }
    [HttpPost]
    public async Task<IActionResult> Post(IFormCollection data)
    {
      Console.WriteLine("New file received");
      var uploads = Path.Combine(Configuration["UploadFolder"]);
      try 
      { 
        var file = data.Files[0];
        var msg = "";
        if (!this.ValidateFile(file, out msg))
        {
          return BadRequest(msg);
        }

        if (!Directory.Exists(uploads))
        {
          Directory.CreateDirectory(uploads);
        }
        if (file.Length > 0)
        {
          var filePath = Path.Combine(uploads, file.FileName);
          using (var fileStream = new FileStream(filePath, FileMode.Create))
          {
            await file.CopyToAsync(fileStream);
          }
          return Ok();
        }
        return BadRequest();
      }
      catch(Exception e)
      {
        Console.WriteLine(e.Message);
        return BadRequest();
      }
    }

    bool ValidateFile(IFormFile file, out string msg)
    {
      msg = "";
      bool res = true;
      if (file.Length > Int32.Parse(Configuration["FileSize"]))
      {
        msg = "File is too big.";
        res = false;
      }
      else if (!Configuration.GetSection($"{"FileFormat"}").Get<List<string>>().Contains(Path.GetExtension(file.FileName)))
      {
        msg = "Unsupported file type.";
        res = false;
      }
      return res;
    }
    private string GetContentType(string path)
    {
      var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
      string contentType;
      if (!provider.TryGetContentType(path, out contentType))
      {
        contentType = "application/octet-stream";
      }
      return contentType;
    }
  }
}
