﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace MUTools
{
    class DBConnection : DataContext
    {
        public Table<DBItems> items;

        public Table<DBItemCategories> categories;

        public DBConnection()  : base("Server=.\\SQLEXPRESS;Database=MuOnline;Integrated Security=true")
        {

        }
    }
}
