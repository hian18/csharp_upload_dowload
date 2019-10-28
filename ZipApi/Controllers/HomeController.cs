using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZipApi.Models;
using ZipApi.Repositories;
namespace ZipApi.Controllers
{
    [Route("api")]
    public class HomeController : Controller
    {
        [Route("home")]
        [HttpGet]
        public string Get() => "teste Home controlle";

        [Route("nome")]
        [HttpGet]
        public string NomeArquivos() => AnexoRepository.anexos[1].Name;

        [Route("upload")]
        [HttpPost]
        public IActionResult Upload([FromForm] IEnumerable<IFormFile> files)
        {

            foreach (var file in files)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {

                    file.CopyTo(memoryStream);

                    var anexo = new Anexo();
                    anexo.file = memoryStream.ToArray();
                    anexo.Name = file.FileName;
                    AnexoRepository.anexos.Add(anexo);


                }

            }
            return Ok();
        }
        [Route("download")]
        [HttpGet]
        public IActionResult Download()
        {
            IList<Anexo> sourceFiles = AnexoRepository.anexos;
            // ...

            // the output bytes of the zip
            byte[] fileBytes = null;

            // create a working memory stream
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                // create a zip
                using (System.IO.Compression.ZipArchive zip = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    // interate through the source files
                    foreach (var f in sourceFiles)
                    {
                        // add the item name to the zip
                        System.IO.Compression.ZipArchiveEntry zipItem = zip.CreateEntry(f.Name);
                        // add the item bytes to the zip entry by opening the original file and copying the bytes 
                        using (System.IO.MemoryStream originalFileMemoryStream = new System.IO.MemoryStream(f.file))
                        {
                            using (System.IO.Stream entryStream = zipItem.Open())
                            {
                                originalFileMemoryStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
                fileBytes = memoryStream.ToArray();
            }

            // download the constructed zip
            Response.Headers.Add("Content-Disposition", "attachment; filename=download.zip");

            // .("Content-Disposition", "attachment; filename=download.zip");
            return File(fileBytes, "application/zip");
        }


        private Stream PackageManyZip(IList<Anexo> anexos)
        {
            byte[] buffer = new byte[65000];
            MemoryStream returnStream = new MemoryStream();
            var zipMs = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(zipMs);
            zipStream.SetLevel(9);
            foreach (var a in anexos)
            {
                string fileName = a.Name;
                // var streamInput = a.file.AsMemory();
                var fileEntry = new ZipEntry(a.Name);
                byte[] fileBytes = a.file;
                fileEntry = new ZipEntry(fileName);
                fileEntry.Size = fileBytes.Length;
                zipStream.PutNextEntry(fileEntry);
                zipStream.Write(fileBytes, 0, fileBytes.Length);

            }
            zipStream.Flush();
            zipStream.Finish();
            zipMs.Position = 0;
            zipMs.CopyTo(returnStream, 5600);


            returnStream.Position = 0;
            return returnStream;
        }


    }


}