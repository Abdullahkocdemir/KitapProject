using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection; 

namespace KitapProject.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("AppUser")]
        public string AppUserId { get; set; } = string.Empty;
        public virtual AppUser AppUser { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "Decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Beklemede"; // Bu property string olarak kalacak

        [Required]
        [StringLength(250)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PostalCode { get; set; } = string.Empty;

        public virtual ICollection<OrderDetail>
    OrderDetails
        { get; set; } = new List<OrderDetail>
        ();
    }

    public enum OrderStatus
    {
        [Display(Name = "Beklemede")]
        Beklemede = 0,
        [Display(Name = "Hazırlanıyor")]
        Hazırlanıyor = 1,
        [Display(Name = "Kargoda")]
        Kargoda = 2,
        [Display(Name = "Teslim Edildi")]
        TeslimEdildi = 3,
        [Display(Name = "İptal Edildi")]
        İptalEdildi = 4
    }

    // Enum Display Name uzantı metotları
    public static class EnumExtensions
    {
        // Enum tipini alan orijinal GetDisplayName metodu
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>
                ()
                ?.GetName() ?? enumValue.ToString();
        }

        // String tipini ve enum tipini alan yeni aşırı yüklenmiş GetDisplayName metodu
        public static string GetDisplayName(this string enumStringValue, Type enumType)
        {
            // Eğer verilen tip bir enum değilse veya null ise hata fırlat
            if (enumType == null || !enumType.IsEnum)
            {
                throw new ArgumentException("enumType bir enum tipi olmalıdır.", nameof(enumType));
            }

            // String değeri enum'a dönüştürmeye çalış
            if (Enum.TryParse(enumType, enumStringValue, true, out object parsedEnum)!)
            {
                // Başarılı olursa, Enum tipini alan metodu çağır
                return ((Enum)parsedEnum).GetDisplayName();
            }

            // Parse edilemezse orijinal string değeri döndür
            return enumStringValue;
        }
    }
}
