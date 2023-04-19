namespace maintenanceServiceAPI.Models;


public class WorkshopRequest
{

    public int ID { get; set; }

    public string Execution { get; set; }


    //Service(false) eller reparation(true)
    public string ServiceType { get; set; }


    public string Customer { get; set; }



    public WorkshopRequest(int iD, string execution, string serviceType, string customer)
    {
        this.ID = iD;
        this.Execution = execution;
        this.ServiceType = serviceType;
        this.Customer = customer;

    }




}