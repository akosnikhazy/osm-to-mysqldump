/// <summary>
/// I had to build a street database to auto fill a HTML field when typing without using any outside API.
/// So I grabbed OpenStreetMaps pbf file, using osmosis I generated a filtered osm file from it, then 
/// using this script I turned that in a MySQL dump.
///
/// My thinking was I can index postal codes and city names like this for speed. But even if I change
/// database structure later I have all the street names connected to settlements and postal codes I need.
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace osm_to_sql
{
    class Program
    {
        static void Main(string[] args)
        {
            // List of every line of the dump
            List<string> SQLLine = new List<string>();

            // first INSERT query
            string sqlPart = "INSERT INTO `tuz_cimek` (`varos`,`iranyitoszam`,`utca`) VALUES "; 
               
            // collecting MySQL insert values in this
            string[] insertData;
           
            int insertValueCounter = 1, // count values in this
                insertValueLimit = 950, // number of values before new INSERT query
                recordCounter = 1;      // counting all records for command line messages

            // loading the osm file
            XmlDocument osm = new XmlDocument();
            os,.Load("your-osm-file.osm");
         
            // basic xml loop, knowing how an osm xml looks like
            foreach (XmlNode node in osm.DocumentElement.ChildNodes)
            {
                // at best we have 3 data in every node, but if there is no data it will stay NULL
                // they represent city, postcode, street
                insertData = new string[3] { "NULL", "NULL", "NULL" };
               
                foreach (XmlNode locNode in node)
                {
                    // there are potentially more than 3 attributes in the node, we only need these three
                    switch (locNode.Attributes["k"].Value)
                    {
                        case "addr:city":
                            insertData[0] = "\"" + locNode.Attributes["v"].Value + "\"";  
                            break;
                        case "addr:postcode":
                            insertData[1] =  "\"" + locNode.Attributes["v"].Value + "\"";
                            break;
                        case "addr:street":
                             insertData[2] = "\"" + locNode.Attributes["v"].Value + "\"";
                             break;
                    }

                }

                insertValueCounter++;
                recordCounter++;

                sqlPart += "(" + String.Join(",", insertData) + ")";

                // to see it work. If you want it faster comment these
                    Console.Clear();
                    Console.WriteLine(recordCounter);
                    Console.WriteLine(sqlPart);

                if (insertValueLimit / insertValueCounter == 1)
                { // start a new INSERT query when it is at limit
                    
                    insertValueCounter = 1;
                    
                    // I put it in this line because at the end we delete duplicates
                    // there could be many duplicates depending on your osm file.
                    // If you put this in its own line at the end it will be deleted.
                    sqlPart += ";INSERT INTO `tuz_cimek` (`varos`,`iranyitoszam`,`utca`) VALUES ";
                    
                    SQLLine.Add(sqlPart);
                    
                    sqlPart = "";
                    
                    continue;
                }
                
                // do not forget to delete the las , from the end of the mysql dump.
                // sorry for the lazy code here, why bother when you use it only once
                sqlPart += ",";
                SQLLine.Add(sqlPart);

                sqlPart = "";
              
            }
            
            // delete duplicate items in the list
            SQLLine = SQLLine.Distinct().ToList();

            // write the list in file
            File.WriteAllLines("your-dump.sql", SQLLine);

            Console.Clear();
            Console.WriteLine("Done! " + recordCounter + " line of data was converted to MySQL");

            var name = Console.ReadLine();
        }
    }
}
