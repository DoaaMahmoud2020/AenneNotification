﻿using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using NotificationApp.API;
using NotificationApp.DTO;
using NotificationApp.DTO.Driver;
using NotificationApp.DTO.Financial;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationApp.Hubs
{
    [HubName("TripNotification")]
    public class TripNotificationHub : Hub
    {
        static List<AllDriverPickUpLocationsDto> AllMatchedDriverVehicalDto = new List<AllDriverPickUpLocationsDto>();
        static List<MatchedDriverVehicalDto> _MatchedDriverVehicalDto = new List<MatchedDriverVehicalDto>();
        
        APIConsume _APIConsume = new APIConsume();
        private static readonly ConcurrentDictionary<string, UserHubModels> Users =
                            new ConcurrentDictionary<string, UserHubModels>(StringComparer.InvariantCultureIgnoreCase);
        

        ///1-Fire Push Rash Hours
        ///3-Push another notification  with Price Cost To Driver
        public async Task<FinancialsRushHoursDto> RequestRashHour(int WeekDay)
        {
            FinancialsRushHoursDto RashHours = new FinancialsRushHoursDto();

            return await Task.Run<FinancialsRushHoursDto>(() =>
            {
                return _APIConsume.GetRashHours(WeekDay);
            }
            );
        }

        public async Task<decimal> RequestPriceCost(PriceCostDto priceCostDto)
        {

            return await Task.Run<decimal>(() =>
            {
                return _APIConsume.CalculatePriceCost(priceCostDto);
            }
          );
            // decimal PriceCost = financialservice.CalculatePriceCost(priceCostDto);
            //await Clients.User(User).ReciveTripCost(PriceCost);

        }

        //////////////////////////
        ///1-Fire Push Get mateched Drivers for  using Healty_number amd Handicapped_Number
        ///3-Push another notification  with Driver Id and Trip_Pickup_Long and Trip_Pickup_Latt of Driver
        public async Task GetMatchedDriver(int Client_Id, int Healty_number, int Handicapped_Number, double Client_Pickup_Long, double Client_Pickup_Latt)
        {
          
             _MatchedDriverVehicalDto = _APIConsume.GetMatchedDriver(Healty_number, Handicapped_Number).ToList();

            //await Clients.All.SendAsync("NotifiedCurrentLongAndLattForDriver");


            for (int i = 0; i < _MatchedDriverVehicalDto.Count; i++)
            {
                //var LstConnIDs = Users.
                //                    Where(x => x.Key == Client_Id.ToString() )
                //                     .Select(x => x.Value).FirstOrDefault();
                 await Clients.All.SendAsync("NotifiedCurrentLongAndLattForDriver", Client_Id, Client_Pickup_Long,  Client_Pickup_Latt);

                //push to specific driver 
                //  await Clients.User(_MatchedDriverVehicalDto[i].Driver_Id.ToString()).SendAsync("NotifiedCurrentLongAndLattForDriver");
                //foreach (var ConnID in LstConnIDs.ConnectionIds)
                //{
                //    await Clients.Client(ConnID).SendAsync("NotifiedCurrentLongAndLattForDriver", Client_Id, Client_Pickup_Long, Client_Pickup_Latt);

                //    // await Clients.All.SendAsync("NotifiedCurrentLongAndLattForDriver", Client_Id, Client_Pickup_Long,  Client_Pickup_Latt);
                //}
            }

        }

      
        public async Task GetCurrentLongAndLattForDriver(int Client_Id,double Client_Pickup_Long,double Client_Pickup_Latt, int Driver_Id, double Driver_Pickup_Long, double Driver_Pickup_Latt)
        {

            if (Driver_Id != 0)
            {


                //AllDriverPickUpLocationsDto DriverPickUpLocations = new AllDriverPickUpLocationsDto();
                //DriverPickUpLocations.Client_Id = Client_Id;
                //DriverPickUpLocations.MatchedDriverVehical.Driver_Id =Driver_Id;
                //DriverPickUpLocations.MatchedDriverVehical.Driver_Pickup_Latt = Driver_Pickup_Latt;
                //DriverPickUpLocations.MatchedDriverVehical.Driver_Pickup_Long = Driver_Pickup_Long;


                AllMatchedDriverVehicalDto.Add(new AllDriverPickUpLocationsDto()
                {
                    Client_Id = Client_Id,
                    MatchedDriverVehical = new MatchedDriverVehicalDto()
                    {
                        Driver_Id = Driver_Id,
                        Driver_Pickup_Latt = Driver_Pickup_Latt,
                        Driver_Pickup_Long = Driver_Pickup_Long
                    }
                });

                //if (AllMatchedDriverVehicalDto.Count == _MatchedDriverVehicalDto.Count)
                //{

                    MatchedDriverVehicalDto nearestdriverobj = GetNearestDriver(Client_Id, Client_Pickup_Long, Client_Pickup_Latt);
                    //  Push Notification To Nearest Driver
                    if (nearestdriverobj != null)
                    {
                        PushToNearestLongAndLattOfDriverForDriver(Client_Id, Driver_Id, Client_Pickup_Long, Client_Pickup_Latt);//nearestdriverobj.Driver_Id, nearestdriverobj.Driver_Pickup_Long, nearestdriverobj.Driver_Pickup_Latt);
                    }
              //  }
            }
        }
        public void PushToNearestLongAndLattOfDriverForDriver(int Client_Id1, int DriverID, double Client_Pickup_Long1, double Client_Pickup_Latt1)
        {
            //push Notification to Driver
              // Clients.User(DriverID.ToString()).SendAsync("NotifiedNearestDriverLongAndLattForDriver",DriverID, Driver_Pickup_Long, Driver_Pickup_Latt);
           //Clients.All.SendAsync("NotifiedNearestDriverLongAndLattForDriver", Client_Id, Driver_Pickup_Long, Driver_Pickup_Latt);
           Clients.All.SendAsync("NotifiedNearestDriverLongAndLattForDriver", Client_Id1, Client_Pickup_Long1, Client_Pickup_Latt1);
        }


        //To Get Nearest Pickup_Long and Trip_Pickup_Latt of   Driver  
        public MatchedDriverVehicalDto GetNearestDriver(int Client_Id, double Client_Pickup_Long, double Client_Pickup_Latt)
        {
            var coord = new GeoCoordinate(Client_Pickup_Long, Client_Pickup_Latt);

            var nearest = AllMatchedDriverVehicalDto.Where(x => x.Client_Id == Client_Id).Select(x => new GeoCoordinate(x.MatchedDriverVehical.Driver_Pickup_Latt, x.MatchedDriverVehical.Driver_Pickup_Long))
                                   .OrderBy(x => x.GetDistanceTo(coord))
                                   .FirstOrDefault();

            MatchedDriverVehicalDto obj = AllMatchedDriverVehicalDto.Where(x => x.MatchedDriverVehical.Driver_Pickup_Latt == nearest.Latitude
                                                                                && x.MatchedDriverVehical.Driver_Pickup_Long == nearest.Longitude
            ).Select(x => new MatchedDriverVehicalDto
            {
                Driver_Id = x.MatchedDriverVehical.Driver_Id,
                Driver_Pickup_Long = x.MatchedDriverVehical.Driver_Pickup_Long,
                Driver_Pickup_Latt = x.MatchedDriverVehical.Driver_Pickup_Latt
            }).FirstOrDefault();
            return obj;
        }


        //// calling when driver accept or reject trip 
        public async Task StatusTripFromDriver(int Client_Id, int Driver_Id, int TripStatusFromDriver, double Client_Pickup_Long, double Client_Pickup_Latt)
        {


            if (TripStatusFromDriver == (int)Enums.TripStatusFromDriver.Accepted)
            {
                 await Clients.User(Client_Id.ToString()).SendAsync("NotifiedForClientByNearestDriver",Driver_Id);
               // await Clients.All.SendAsync("NotifiedForClientByNearestDriver",Driver_Id);

            }
            else // Rejected or TimerEnded
            {
                MatchedDriverVehicalDto Nextdriverobj = GetNextNearestDriver(Client_Id, Client_Pickup_Long, Client_Pickup_Latt, Driver_Id);
                //  Push Notification To Next Driver
                if (Nextdriverobj != null)
                {
                    PushToNearestLongAndLattOfDriverForDriver(Client_Id, Nextdriverobj.Driver_Id, Nextdriverobj.Driver_Pickup_Long, Nextdriverobj.Driver_Pickup_Latt);
                }
            }
        }

        //to Get Next Nearest Driver
        public MatchedDriverVehicalDto GetNextNearestDriver(int Client_Id, double Client_Pickup_Long, double Client_Pickup_Latt, int Driver_Id)
        {

            //Get all Drivers from list expect specific driver 
            List<AllDriverPickUpLocationsDto> ListNextMatchedDriver = AllMatchedDriverVehicalDto.Where(a => a.Client_Id == Client_Id && a.MatchedDriverVehical.Driver_Id != Driver_Id).ToList();

            var coord = new GeoCoordinate(Client_Pickup_Long, Client_Pickup_Latt);

            var nearest = ListNextMatchedDriver.Where(q => q.Client_Id == Client_Id).Select(x => new GeoCoordinate(x.MatchedDriverVehical.Driver_Pickup_Latt, x.MatchedDriverVehical.Driver_Pickup_Long))
                                   .OrderBy(x => x.GetDistanceTo(coord))
                                   .FirstOrDefault();

            MatchedDriverVehicalDto obj = ListNextMatchedDriver.Where(x => x.MatchedDriverVehical.Driver_Pickup_Latt == nearest.Latitude
                                                                                && x.MatchedDriverVehical.Driver_Pickup_Long == nearest.Longitude
            ).Select(x => new MatchedDriverVehicalDto
            {
                Driver_Id = x.MatchedDriverVehical.Driver_Id,
                Driver_Pickup_Long = x.MatchedDriverVehical.Driver_Pickup_Long,
                Driver_Pickup_Latt = x.MatchedDriverVehical.Driver_Pickup_Latt
            }).FirstOrDefault();
            return obj;
        }
        ///Get next driver after 30 sec

        // to tracking driver in map 
        public async Task DriverOnlineTracking(int Driver_Id)
        {
            //push to specific driver 
          //  await Clients.All.SendAsync("NotifiedCurrentLongAndLattForDriver");
           await Clients.User(Driver_Id.ToString()).SendAsync("NotifiedGetCurrentLongAndLattForDriver");
        }
        public async Task GetCurrentLongAndLattForSpecificDriver(int Client_Id, int Driver_Id, double Driver_Pickup_Long, double Driver_Pickup_Latt)
        {

            if (Driver_Id != 0)
            {
                //push Notification to client
                 await Clients.User(Client_Id.ToString()).SendAsync("NotifiedCurrenDriverLocationForClient",Driver_Id, Driver_Pickup_Long, Driver_Pickup_Latt);
                //  await Clients.All.SendAsync("NotifiedCurrenDriverLocationForClient",Driver_Id, Driver_Pickup_Long, Driver_Pickup_Latt);
            }
        }
        // to clear  static list for specfic client when trip is Ended
        public async Task EndedTripForClient(int Client_Id)
        {
            AllMatchedDriverVehicalDto.Remove(AllMatchedDriverVehicalDto.Single(s => s.Client_Id == Client_Id));
        }
        public async Task OnConnectedAsync(string UserID,int UserType)
        {
            //string userName = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;

            var user = Users.GetOrAdd(UserID, _ => new UserHubModels
            {
                UserID = UserID,
                UserType= UserType,
                ConnectionIds = new HashSet<string>()
            });

            //lock (user.ConnectionIds)
            //{
            //    user.ConnectionIds.Add(connectionId);
            //    if (user.ConnectionIds.Count == 1)
            //    {
            //        Clients.Others.SendAsync("userConnected",UserID);
            //    }
            //}

           // return base.OnConnectedAsync();
        }

        public async Task OnDisconnectedAsync(string UserID,int UserType)
        {
            string userName = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;

            UserHubModels user;
            Users.TryGetValue(userName, out user);

            if (user != null)
            {
                lock (user.ConnectionIds)
                {
                    user.ConnectionIds.RemoveWhere(cid => cid.Equals(connectionId));
                    //if (!user.ConnectionIds.Any())
                    //{
                    //    UserHubModels removedUser;
                    //    Users.TryRemove(userName, out removedUser);
                    //    //Clients.Others.userDisconnected(userName);
                    //    Clients.Others.SendAsync("userDisconnected", userName);

                    //}
                }
            }

          // return base.OnDisconnectedAsync(UserID,UserType);
        }
    

}
}
