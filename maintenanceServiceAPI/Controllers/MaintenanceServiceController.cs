using Microsoft.AspNetCore.Mvc;
using maintenanceServiceAPI.Models;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace maintenanceServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class MaintenanceController : ControllerBase
{


    private readonly ILogger<MaintenanceController> _logger;



    private ConnectionFactory factory = new ConnectionFactory();
    private IConnection connection;
    private IModel channel;

    private string _planPath = string.Empty;


    public MaintenanceController(ILogger<MaintenanceController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _planPath = configuration["PlanPath"] ?? String.Empty;

        string connectionString = configuration["RabbitMQConnectionString"] ?? string.Empty;

        _logger.LogInformation($"PlanPath is {_planPath}");
        _logger.LogInformation($"connection string is {connectionString}");

        factory = new ConnectionFactory() { HostName = connectionString };
        connection = factory.CreateConnection();   //Selve forbindelsen skal kun køres 1 gang i constructoren. -Vigtigt at den kun ligger her.
        channel = connection.CreateModel();

        _logger.LogInformation("Maintenance started");
    }



    [HttpGet("GetRepairPlan")]

    [ProducesResponseType(typeof(WorkshopRequest), StatusCodes.Status200OK)]        //med lidt hjælp fra model-klassen CSVreader
                                                                                    //fordi controller-basen ikke kan læse i computerens filsystem
    public IActionResult GetRepairPlanCSV()
    {
        CSVreader service = new CSVreader();
        try
        {
            List<WorkshopRequest> bookings = service.ReadCSV(_planPath);
            return Ok(bookings);
        }

        catch (Exception)
        {
            return StatusCode(410, $" - It's gone daddy.");
        }
    }


    [HttpPost("PostWorkshopRequest")]
    //En post der tilføjer en Maintenance
    public IActionResult PostWorkshopRequest([FromBody] WorkshopRequest workshopRequest)
    {
        try
        {
            _logger.LogInformation("WorkshopRequest oprettet" + StatusCodes.Status200OK,    //status 200 er ok
            DateTime.UtcNow.ToLongTimeString());
            channel.ExchangeDeclare(workshopRequest.ServiceType, ExchangeType.Topic);
           

            string message = JsonConvert.SerializeObject(workshopRequest);
            var body = Encoding.UTF8.GetBytes(message);

            // ServiceType datatype String skal være "Reparation" eller "Service" !!!
            channel.BasicPublish(exchange: workshopRequest.ServiceType,
                                 routingKey: "hello",
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation($"WorkshopRequest added - {message}");
        
        
           
            return Ok(message);
        }

        catch (Exception)                                                        //fanger exceptions med en log og en DateTime
        {
            _logger.LogInformation("Fejl, WorkshopRequest ikke oprettet",
            DateTime.UtcNow.ToLongTimeString());

            return null;
        }
    }
}
