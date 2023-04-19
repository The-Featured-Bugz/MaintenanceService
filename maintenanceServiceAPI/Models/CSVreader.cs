using System;
namespace maintenanceServiceAPI.Models;

	public class CSVreader
	{

        //Controlleren kan ikke nedarve adgang til fil-bibliotekerne

		public List<WorkshopRequest> ReadCSV(string path)
		{
            List<WorkshopRequest> fulllist = new List<WorkshopRequest>();
            var lines = File.ReadAllLines(path);    //StringArray
            foreach (var line in lines)    //Hver linje er et element i array'et
            {
                var values = line.Split(',');   //Værdierne i arrayet er kommasepareret


                //Behøves ikke hvis det bar eskal printes som tekst.
              //  var plan = new WorkshopRequest(int.Parse(values[0]), DateTime.Parse(values[1]), values[2], values[3], values[4], values[5], DateTime.Parse(values[6]));
               var plan= new WorkshopRequest(Convert.ToInt32(values[0]), values[1],values[2],values[3]); //Bliver nødt til at parse for at kunne få nogle værdier.

                fulllist.Add(plan);     
            }
            return fulllist;
        }


		public CSVreader()
		{

		}
	}

