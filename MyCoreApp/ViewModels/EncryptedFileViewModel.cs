using System.ComponentModel.DataAnnotations;

namespace MyCoreApp.ViewModels
{
    public class EncryptedFileViewModel
    {
        [Required(ErrorMessage = "Виберіть файл(и).")]
        [Display(Name = "Перетягніть файли сюди або клацніть у цій області.")]
        public List<IFormFile> UploadsDecrypt { get; set; }//Список завантажених файлів
        [Required(ErrorMessage = "Введіть ключ.")]
        public string Key { get; set; }
    }
}
