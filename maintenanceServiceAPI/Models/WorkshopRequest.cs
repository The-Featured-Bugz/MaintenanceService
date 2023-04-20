namespace maintenanceServiceAPI.Models;


public class WorkshopRequest
{

    public int ID { get; set; }

    public string Execution { get; set; }


    //Service eller Repair
    public string ServiceType { get; set; }


    public string Customer { get; set; }

    public DateTime CurrentDate {get; set;}

    public WorkshopRequest(int iD, string execution, string serviceType, string customer, DateTime currentDate)
    {
        this.ID = iD;
        this.Execution = execution;
        this.ServiceType = serviceType;
        this.Customer = customer;
        this.CurrentDate = currentDate;

    }




}