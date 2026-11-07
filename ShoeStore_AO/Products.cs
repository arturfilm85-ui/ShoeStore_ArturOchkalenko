namespace ShoeStore_AO
{
    using System;
    using System.Collections.Generic;
    
    public partial class Products
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Products()
        {
            this.OrderItems = new HashSet<OrderItems>();
        }
    
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public int SupplierID { get; set; }
        public int ManufacturerID { get; set; }
        public int CategoryID { get; set; }
        public decimal Discount { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; }
        public string Photo { get; set; }
    
        public virtual Categories Categories { get; set; }
        public virtual Manufacturers Manufacturers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderItems> OrderItems { get; set; }
        public virtual Suppliers Suppliers { get; set; }
        public bool is_discount => Discount > 15;

        public bool is_on_sale => Discount > 0;

        public decimal? price_discount
        {
            get
            {
                return Price - (Price * Discount / 100);
            }
        }


        public bool is_quantity => StockQuantity <= 0;
        
        public string FullPhotoPath
        {
            get
            {
                if (string.IsNullOrEmpty(Photo))
                    return "Resources/picture.png";
                return System.IO.Path.Combine(
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Images"),
                    Photo);
            }
        }
    }
}
