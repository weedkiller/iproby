//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace iproby.Data_Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class customer_announ
    {
        public int id { get; set; }
        public Nullable<int> customer_id { get; set; }
        public Nullable<int> announ_id { get; set; }
        public System.DateTime date_from { get; set; }
        public System.DateTime date_to { get; set; }
        public int active { get; set; }
    
        public virtual announ announ { get; set; }
        public virtual customer customer { get; set; }
    }
}
