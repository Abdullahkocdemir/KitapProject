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
        public string OrderStatus { get; set; } = "Beklemede"; 

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

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>
                ()
                ?.GetName() ?? enumValue.ToString();
        }

        public static string GetDisplayName(this string enumStringValue, Type enumType)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                throw new ArgumentException("enumType bir enum tipi olmalıdır.", nameof(enumType));
            }

            if (Enum.TryParse(enumType, enumStringValue, true, out object parsedEnum))
            {
                return ((Enum)parsedEnum).GetDisplayName();
            }

            return enumStringValue;
        }
    }
}
