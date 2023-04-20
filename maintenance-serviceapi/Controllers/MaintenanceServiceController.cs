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

    private string _csvLocation = string.Empty;


    public MaintenanceController(ILogger<MaintenanceController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _csvLocation = configuration["csvLocation"] ?? String.Empty;

        string connectionString = configuration["RabbitMQConnectionString"] ?? string.Empty;

        _logger.LogInformation($"csvLocation is {_csvLocation}");
        _logger.LogInformation($"connection string is {connectionString}");

        factory = new ConnectionFactory() { HostName = connectionString };
        connection = factory.CreateConnection();   //Selve forbindelsen skal kun køres 1 gang i constructoren. -Vigtigt at den kun ligger her.
        channel = connection.CreateModel();

        _logger.LogInformation("Maintenance started");

        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"Maintenance responding from {_ipaddr}");
    }

    // Angiver, at denne metode vil håndtere HTTP GET-forespørgsler på en bestemt URL.
    [HttpGet("Get{serviceType}plan")]

    // Angiver, at hvis anmodningen er vellykket, vil HTTP-statuskoden i svaret være 200 OK, og typen af ​​data, der returneres, vil være WorkshopRequest.
    [ProducesResponseType(typeof(WorkshopRequest), StatusCodes.Status200OK)]

    // Definerer en metode, der vil håndtere GET-forespørgsler på denne URL, med en parameter kaldet "serviceType".
    public IActionResult GetServiceTypePlanCSV(string serviceType)
    {
        // Opretter en ny CSVreader-instans til at læse data fra CSV-filer.
        CSVreader service = new CSVreader();

        try
        {

            // Læser data fra en CSV-fil med navnet på den påkrævede serviceType og returnerer en liste over WorkshopRequest-objekter.
            List<WorkshopRequest> work = service.ReadCSV("C:\\Users\\jacob\\Music\\MicroProject\\PlanningService" + "\\" + serviceType + ".csv");
            DateTime today = DateTime.Today;
            //Tilføjer alle "work" som matcher med datoen idag ind i "todayWork".
            List<WorkshopRequest> todayWork = work.FindAll(x => x.CurrentDate.ToString("dd") == today.ToString("dd"));

            _logger.LogInformation($"Return todayWork.Count: {todayWork.Count}");
            // Returnerer en HTTP 200 OK status og en liste over WorkshopRequest-objekter.
            return Ok(todayWork);
        }
        catch (Exception)
        {
            _logger.LogInformation($" - It's gone daddy.");
            // Hvis der opstår en fejl, returneres en HTTP-statuskode på 410, der angiver, at den anmodede ressource er permanent fjernet fra serveren.
            return StatusCode(410, $" - It's gone daddy.");
        }
    }


    [HttpPost("PostWorkshopRequest")]
    // HTTP POST-metode, der tilføjer en Maintenance
    public IActionResult PostWorkshopRequest([FromBody] WorkshopRequest workshopRequest)
    {
        try
        {
            _logger.LogInformation("WorkshopRequest oprettet" + StatusCodes.Status200OK,    // Logger en information om, at WorkshopRequest er oprettet, med HTTP status 200 OK og tidspunktet.
            DateTime.UtcNow.ToLongTimeString());

            // Exchange(topic_logs) bestemmer, hvilken type meddelelse der sendes til hvilken kø "Repair" eller "Service".
            channel.ExchangeDeclare(exchange: "topic_logs", ExchangeType.Topic);

            string message = JsonConvert.SerializeObject(workshopRequest);    // Konverterer WorkshopRequest til en JSON-streng.
            var body = Encoding.UTF8.GetBytes(message);    // Konverterer JSON-strengen til en byte-array.

            // ServiceType datatype String skal være "Repair" eller "Service" !!!
            channel.BasicPublish(exchange: "topic_logs",
                                 // Sender beskeden til køen, der passer til ServiceType, som kan være "Repair" eller "Service".
                                 routingKey: workshopRequest.ServiceType,
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation($"WorkshopRequest added - {message}");    // Logger en information om, at WorkshopRequest er tilføjet med WorkshopRequest JSON-strengen.

            return Ok(message);    // Returnerer HTTP status 200 OK med WorkshopRequest JSON-strengen i responskroppen.
        }
        catch (Exception)    // Håndterer exceptions og logger dem med tidspunktet.
        {
            _logger.LogInformation("Fejl, WorkshopRequest ikke oprettet",
            DateTime.UtcNow.ToLongTimeString());

            return null;
        }
    }

}
