﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Data.DataTransfer.Record
{
    public class RecordGetById
    {
        public int Id { get; set; }

        public string ModelFileName { get; set; }

        public int RecordTypeId { get; set; }

        public string RecordType { get; set; }
    }
}
