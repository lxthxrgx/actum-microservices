using System;
using System.Collections.Generic;
using System.Text;
using SharedLibraries.model;

namespace SharedLibraries.model
{
    public class GroupModel
    {
        public int Id { get; set; }
        public int NumberGroup { get; set; }
        public int CounterpartyId { get; set; }
        public CounterpartyModel Counterparty { get; set; } = new CounterpartyModel();
        public string Address { get; set; } = "Адреса";
        public double Area { get; set; } = 1.0;
        public bool IsAlert { get; set; } = false;
        public DateTime DateCloseDepartment { get; set; } = DateTime.MinValue;
        public ICollection<GuardModel> Guard { get; set; } = new List<GuardModel>();
        public ICollection<SubleaseModel> Sublease { get; set; } = new List<SubleaseModel>();
    }
}
