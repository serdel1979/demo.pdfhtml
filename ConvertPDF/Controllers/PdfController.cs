using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;




using HtmlAgilityPack;
//using Aspose.Pdf;
using ConvertPDF.Models;

using SkiaSharp;
using UglyToad.PdfPig;
//using Aspose.Pdf.Text;


namespace ConvertPDF.Controllers
{
    public class PdfController : Controller
    {
        // GET: PdfController
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);

                }
                string htmlContent = ConvertPdfToHtml(path);
                var model = new PdfViewModel { HtmlContent = htmlContent };
                return View("Display", model);
            }

            return RedirectToAction("Index");
        }


        private string ConvertPdfToHtml(string pdfPath)
        {
            using var pdfDocument = PdfDocument.Open(pdfPath);
            var outputHtml = new StringWriter();

            foreach (var page in pdfDocument.GetPages())
            {
                outputHtml.WriteLine($"<h2>Page {page.Number}</h2>");

                // Extraer texto de la página
                var pageText = page.Text;
                outputHtml.WriteLine($"<p>{pageText}</p>");

                var imageCounter = 1;

                foreach (var image in page.GetImages())
                {
                    using (var imageStream = new MemoryStream((byte[])image.RawBytes))
                    {
                        using (var skBitmap = SKBitmap.Decode((byte[])image.RawBytes))
                        using (var skImage = SKImage.FromBitmap(skBitmap))
                        using (var skData = skImage.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            var imagePath = Path.Combine("wwwroot", $"page_{page.Number}_image_{imageCounter}.png");
                            using (var imageFileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
                            {
                                skData.SaveTo(imageFileStream);
                            }
                            outputHtml.WriteLine($"<img src='/page_{page.Number}_image_{imageCounter}.png' />");
                            imageCounter++;
                        }
                    }
                }
            }

            return outputHtml.ToString();
        }





        /*
        private string ConvertPdfToHtml(string pdfPath)
        {
            var pdfDocument = new Document(pdfPath);
            var outputHtml = new StringWriter();

            foreach (var page in pdfDocument.Pages)
            {
                outputHtml.WriteLine($"<h2>Page {page.Number}</h2>");
                var imageCounter = 1;

                foreach (var image in page.Resources.Images)
                {
                    using (var imageStream = new MemoryStream())
                    {
                        image.Save(imageStream);
                        var imageBytes = imageStream.ToArray();

                        using (var skBitmap = SKBitmap.Decode(imageBytes))
                        using (var skImage = SKImage.FromBitmap(skBitmap))
                        using (var skData = skImage.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            var imagePath = Path.Combine("wwwroot", $"page_{page.Number}_image_{imageCounter}.png");
                            using (var imageFileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
                            {
                                skData.SaveTo(imageFileStream);
                            }
                            outputHtml.WriteLine($"<img src='/page_{page.Number}_image_{imageCounter}.png' />");
                            imageCounter++;
                        }
                    }
                }
            }

            return outputHtml.ToString();
        }
        */
        /*
                private string ConvertPdfToHtml(string pdfPath)
                {
                    var doc = new HtmlDocument();
                    var body = doc.DocumentNode.AppendChild(doc.CreateElement("body"));

                    Document pdfDocument = new Document(pdfPath);
                    int imageCounter = 1;

                    foreach (var page in pdfDocument.Pages)
                    {
                        // Extraer texto
                        var absorber = new Aspose.Pdf.Text.TextAbsorber();
                        page.Accept(absorber);
                        string text = absorber.Text;
                        var textNode = doc.CreateElement("p");
                        textNode.InnerHtml = text;
                        body.AppendChild(textNode);

                        // Extraer imágenes
                        foreach (XImage image in page.Resources.Images)
                        {
                            using (MemoryStream imageStream = new MemoryStream())
                            {
                                image.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
                                string imagePath = SaveImage(imageStream, page.Number, imageCounter++);
                                var imgNode = doc.CreateElement("img");
                                imgNode.SetAttributeValue("src", Url.Content(imagePath));
                                body.AppendChild(imgNode);
                            }
                        }

                        // Reiniciar el contador de imágenes para la siguiente página
                        imageCounter = 1;
                    }

                    using (StringWriter writer = new StringWriter())
                    {
                        doc.Save(writer);
                        return writer.ToString();
                    }
                }


                */


        private string SaveImage(MemoryStream imageStream, int pageNumber, int imageCounter)
        {
            string imagePath = Path.Combine("wwwroot/images", $"Page{pageNumber}_Image{imageCounter}.png");
            string relativePath = $"/images/Page{pageNumber}_Image{imageCounter}.png";

            using (var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
            {
                imageStream.WriteTo(fileStream);
            }

            return relativePath;
        }













    }
}
