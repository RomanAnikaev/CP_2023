using Microsoft.AspNetCore.Mvc;
using MyCoreApp.Models;
using MyCoreApp.ViewModels;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;


namespace MyCoreApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string MyKey = "567fgh890";
        private readonly string _uploadsFolder = "wwwroot/uploadedFiles";
        private readonly string _uploadsEncrypFolder = "wwwroot/uploadedEncrypFiles";
        private readonly string _encryptedsFolder = "wwwroot/encryptedFiles";
        private readonly string _decryptedsFolder = "wwwroot/decryptedFiles";
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }
        [HttpGet]
        public IActionResult Privacy()
        {
            string dateTime = DateTime.Now.ToString("d", new CultureInfo("en-US"));
            ViewData["TimeStamp"] = dateTime;
            ViewData["Title"] = "Privacy Policy";

            return View();
        }
        [HttpGet]
        public IActionResult Dowload()
        {
            var model = GetFilesList();
            return View(model);
        }
        [HttpGet]
        public IActionResult DowloadDecrypt()
        {
            var model = GetDecryptFilesList();
            return View(model);
        }
        [HttpGet]
        public IActionResult Decryption()
        {
            return View();
        }
        //=================================================================================================================
        //Завантаження файлів на сервер
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> UploadFiles([FromServices] IWebHostEnvironment environment, UploadFileViewModel ufv)
        {
            if (ModelState.IsValid)
            {
                long size = 0;//розмір файлу
                var files = ufv.Uploads;
                string InputKey = ufv.Key;
                if (InputKey == MyKey) {//Перевірка ключа
                    foreach (var file in files)
                    {
                        //папка зберігання файлів
                        string uploadPath = Path.Combine(environment.WebRootPath, "uploadedFiles", file.FileName);
                        //папка зберігання зашифрованів файлів
                        string encryptedPath = Path.Combine(environment.WebRootPath, "encryptedFiles", GetEncryptedFileName(file.FileName));


                        using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        //Шифрування цих файлів
                        EncryptFile(uploadPath, encryptedPath);

                        size += file.Length;
                    }
                    string message = $"{files.Count} файл(ів) / {size} байтів завантажено та успішно зашифровано!";
                    ViewBag.Message = message;//Вивід результату на сайт
                    //return Json(message);
                }
                else
                {
                    ViewBag.Message = "Вибачте, але данний ключ не є дійсним!";
                }
            }
            return View(nameof(Index), ufv);
        }

        //=================================================================================================================
        //Завантаження зашифрованіх файлів на сервер
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DecryptFiles([FromServices] IWebHostEnvironment environment, EncryptedFileViewModel ufv)
        {
            if (ModelState.IsValid)
            {
                long size = 0;//розмір файлу
                var files = ufv.UploadsDecrypt;
                string InputKey = ufv.Key;
                if (InputKey == MyKey)
                {//Перевірка ключа
                    foreach (var file in files)
                    {
                        //папка зберігання завантажених зашифрованіх файлів
                        string uploadedEncrypPath = Path.Combine(environment.WebRootPath, "uploadedEncrypFiles", file.FileName);
                        //папка зберігання дешифрованів файлів
                        string decryptedPath = Path.Combine(environment.WebRootPath, "decryptedFiles", GetDecryptedFileName(file.FileName));


                        using (var fileStream = new FileStream(uploadedEncrypPath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        //Шифрування цих файлів
                        DecryptFile(uploadedEncrypPath, decryptedPath);

                        size += file.Length;
                    }

                    string message = $"{files.Count} файл(ів) / {size} байтів завантажено та успішно розшифровано!";
                    ViewBag.Message = message;
                    //return Json(message);
                }
                else
                {
                    ViewBag.Message = "Вибачте, але данний ключ не є дійсним!";
                }
            }
            return View(nameof(Decryption), ufv);
        }

        //=================================================================================================================
        //Вивід зашифрованих файлів на сайт та іх завантаження
        public IActionResult DownloadEncryptFile(string fileName, int intValue)
        {
            //папка зберігання зашифрованих файлів
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _encryptedsFolder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }


            return NotFound();
        }
        //=================================================================================================================
        //Вивід дешифрованих файлів на сайт та іх завантаження
        public IActionResult DownloadDecryptFile(string fileName, int intValue)
        {
            //папка зберігання дешифрованих файлів
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _decryptedsFolder, fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }


            return NotFound();
        }
        //=================================================================================================================
        //отримання списку зашифрованіх файлів
        private List<string> GetFilesList()
        {
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), _encryptedsFolder);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var files = Directory.GetFiles(directoryPath);
            var fileNames = new List<string>();

            foreach (var file in files)
            {
                fileNames.Add(Path.GetFileName(file));
            }

            return fileNames;
        }
        //=================================================================================================================
        //отримання списку дешифрованіх файлів
        private List<string> GetDecryptFilesList()
        {
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), _decryptedsFolder);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var files = Directory.GetFiles(directoryPath);
            var fileNames = new List<string>();

            foreach (var file in files)
            {
                fileNames.Add(Path.GetFileName(file));
            }

            return fileNames;
        }

        //=================================================================================================================
        //Шифрування файлів
        private static void EncryptFile(string inputPath, string outputPath)
        {
            int shift1 = 6; //ключ для Шифрування Цезаря

            using (FileStream inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            using (FileStream outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        byte originalByte = buffer[i];
                        byte shiftedByte = (byte)(originalByte + shift1);
                        // Обмежуємо значення байта, щоб не вийти за межі допустимого діапазону (0-255)
                        shiftedByte = (byte)Math.Min(shiftedByte, byte.MaxValue);

                        buffer[i] = shiftedByte;
                    }

                    outputStream.Write(buffer, 0, bytesRead);
                }
            }
        }
        //=================================================================================================================
        //Дешифрування файлів
        private static void DecryptFile(string inputPath, string outputPath)
        {
            int shift1 = 6; // кількість позицій здвигу

            using (FileStream inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            using (FileStream outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        byte shiftedByte = buffer[i];
                        byte originalByte = (byte)(shiftedByte - shift1);

                        // Обмежуємо значення байта, щоб не вийти за межі допустимого діапазону (0-255)
                        originalByte = (byte)Math.Max(originalByte, byte.MinValue);

                        buffer[i] = originalByte;
                    }

                    outputStream.Write(buffer, 0, bytesRead);
                }
            }
        }
        //=================================================================================================================
        // Метод для отримання зашифрованого імени файлу
        private string GetEncryptedFileName(string originalFileName)
        {
            return "encrypted_" + originalFileName;
        }
        //=================================================================================================================
        // Метод для отримання дешифрованого імени файлу
        private string GetDecryptedFileName(string originalFileName)
        {
            return "decrypted_" + originalFileName;
        }
        //=================================================================================================================
        //Вивід стоорінки помилок
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}