using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PchelaMap.Models;

namespace PchelaMap.Areas.Identity.Data
{
    public class JSONgenerator
    {
        private PchelaMapUser user;
        private bool isSignedIn;
        private string webRoot;

        public JSONgenerator(PchelaMapUser _user, bool _isSignedIn, string _webRoot)
        {
            user = _user;
            isSignedIn = _isSignedIn;
            webRoot = _webRoot;
        }
       
        public async void jsonGenerator(AllUsers allList)
        {
            IList<MapFeature> UsersFeatures = (from User uservar in allList.UsersList
                                               select new MapFeature
                                               {

                                                   type = "Feature",
                                                   id = uservar.id,
                                                   geometry = new MapFeatureGeometry()
                                                   {
                                                       type = "Point",
                                                       coordinates = new List<string>
                                                        {
                                                           uservar.CoordinateX,
                                                           uservar.CoordinateY
                                                        }
                                                   },
                                                   properties = new MapFeatureProperties()
                                                   { hintContent = uservar.Name + ": " + uservar.Adress },
                                                   options = new MapFeatureOptions()
                                                   {
                                                       iconLayout = "default#imageWithContent",
                                                       iconImageHref = "/Images/EmptyUserRound.png",
                                                       fillImageHref = uservar.PhotoUrl,
                                                       iconShape = new MapFeatureIconShape()
                                                       {
                                                           type = "Circle",
                                                           coordinates = new int[] { 0, 0 },
                                                           radius = 25
                                                       },

                                                       iconImageSize = new int[] { 48, 48 },
                                                       iconImageOffset = new int[] { -24, -24 }
                                                   }
                                               }).ToList();
            UsersJsonStruct UsersObjectsJson = new UsersJsonStruct()
            {
                type = "FeatureCollection",
                features = UsersFeatures
            };
            string jsonpath = "";
            if (isSignedIn)
            {
                jsonpath = user.Id;
            }
            else
            {
                jsonpath = "Unregistered";
            }
            if (!Directory.Exists(Path.Combine(webRoot, "js/jsons/" + jsonpath)))
            {
                Directory.CreateDirectory(Path.Combine(webRoot, "js/jsons/" + jsonpath));
            }
            using (FileStream fs = System.IO.File.Create(Path.Combine(webRoot, "js/jsons/" + jsonpath + "/UsersGeoObjects.json")))
            {
                await JsonSerializer.SerializeAsync(fs, UsersObjectsJson);
            };

            IList<MapFeature> TasksFeatures = (from UserWithTasks uservar in allList.UsersTaskList
                                               select new MapFeature
                                               {
                                                   type = "Feature",
                                                   id = uservar.id,
                                                   geometry = new MapFeatureGeometry()
                                                   {
                                                       type = "Point",
                                                       coordinates = new List<string>
                                                   {

                                                      uservar.CoordinateX,
                                                      uservar.CoordinateY
                                                   }
                                                   }
                                                   ,
                                                   properties = new MapFeatureProperties()
                                                   { hintContent = uservar.Adress },
                                                   options = new MapFeatureOptions()
                                                   {
                                                       iconLayout = "default#imageWithContent",
                                                       iconImageHref = "/Images/Empty.png",
                                                       iconImageSize = new int[] { 48, 48 },
                                                       iconImageOffset = new int[] { -24, -24 }
                                                   }
                                               }).ToList();
            UsersJsonStruct TasksObjectsJson = new UsersJsonStruct()
            {
                type = "FeatureCollection",
                features = TasksFeatures
            };

            using (FileStream fs = new FileStream(Path.Combine(webRoot, "js/jsons/" + jsonpath + "/TasksGeoObjects.json"), FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(fs, TasksObjectsJson);
            };
            IList<MapFeature> UrgentTasksFeatures = (from UserWithTasks uservar in allList.UrgentTasksList
                                                     select new MapFeature
                                                     {
                                                         type = "Feature",
                                                         id = uservar.id,
                                                         geometry = new MapFeatureGeometry()
                                                         {
                                                             type = "Point",
                                                             coordinates = new List<string>
                                                   {

                                                      uservar.CoordinateX,
                                                      uservar.CoordinateY
                                                   }
                                                         }
                                                         ,
                                                         properties = new MapFeatureProperties()
                                                         { hintContent = uservar.Adress },
                                                         options = new MapFeatureOptions()
                                                         {
                                                             iconLayout = "default#imageWithContent",
                                                             iconImageHref = "/Images/Empty.png",
                                                             iconImageSize = new int[] { 48, 48 },
                                                             iconImageOffset = new int[] { -24, -24 }
                                                         }
                                                     }).ToList();
            UsersJsonStruct UrgentTasksObjectsJson = new UsersJsonStruct()
            {
                type = "FeatureCollection",
                features = UrgentTasksFeatures
            };

            using (FileStream fs = new FileStream(Path.Combine(webRoot, "js/jsons/" + jsonpath + "/UrgentTasksGeoObjects.json"), FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(fs, UrgentTasksObjectsJson);
            };
            IList<MapFeature> DoneTasksFeatures = (from UserWithTasks uservar in allList.DoneTasksList
                                                   select new MapFeature
                                                   {
                                                       type = "Feature",
                                                       id = uservar.id,
                                                       geometry = new MapFeatureGeometry()
                                                       {
                                                           type = "Point",
                                                           coordinates = new List<string>
                                                   {

                                                      uservar.CoordinateX,
                                                      uservar.CoordinateY
                                                   }
                                                       }
                                                       ,
                                                       properties = new MapFeatureProperties()
                                                       { hintContent = uservar.Adress },
                                                       options = new MapFeatureOptions()
                                                       {
                                                           iconLayout = "default#imageWithContent",
                                                           iconImageHref = "/Images/Empty.png",
                                                           iconImageSize = new int[] { 48, 48 },
                                                           iconImageOffset = new int[] { -24, -24 }
                                                       }
                                                   }).ToList();
            UsersJsonStruct DoneTasksObjectsJson = new UsersJsonStruct()
            {
                type = "FeatureCollection",
                features = DoneTasksFeatures
            };

            using (FileStream fs = new FileStream(Path.Combine(webRoot, "js/jsons/" + jsonpath + "/DoneTasksObjectsJson.json"), FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(fs, DoneTasksObjectsJson);
            };
        }
    }
}
