using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.XConnect.Collection.Model;
using Serilog;

namespace Sitecore.XConnect.ServicePlugins.InteractionsTracker
{
    public class DataExportService
    {
        public static void SendContactInteraction(string contactId)
        {
            try
            {
                var contact = new Models.ContactNew()
                {
                    contact = contactId
                };

                using (var adapter = new PowerBIAdapter())
                {
                    adapter.PushRow(contact, "Contact");
                }

            }
            catch (Exception e)
            {
                Log.Error("Error during Export data to Power BI. Error: {0}", e.Message);
            }
        }
        public static void SendContactInteraction(Interaction entity)
        {
            try
            {
                var contact = new Models.Contact()
                {
                    Id = entity.Contact.Id.Value.ToString()
                };

                var interaction = new Models.Interaction()
                {
                    Id = entity.Id.Value.ToString(),
                    CampaignId = entity.CampaignId?.ToString(),
                    ChannelId = entity.ChannelId.ToString(),
                    ConcurrencyToken = entity.ConcurrencyToken?.ToString(),
                    ContactId = entity.Contact.Id?.ToString(),
                    Duration = (int)entity.Duration.TotalMilliseconds,
                    EndDataTime = entity.EndDateTime,
                    StartDataTime = entity.StartDateTime,
                    EngagementValue = entity.EngagementValue,
                    VenueId = entity.VenueId?.ToString()
                };

                var eventList = entity.Events.Select(x => new Models.Event()
                {
                    Id = x.Id.ToString(),
                    DefenitionId = x.DefinitionId.ToString(),
                    InteractionId = entity.Id.Value.ToString(),
                    ItemId = x.ItemId.ToString(),
                    EngagementValue = x.EngagementValue,
                    Text = x.Text,
                    Duration = (int)x.Duration.TotalMilliseconds,
                    Timestamp = x.Timestamp
                });

                using (var adapter = new PowerBIAdapter())
                {
                    adapter.PushRow(interaction, "Interaction");
                    adapter.PushRow(contact, "Contact");
                    adapter.PushRows(eventList, "Event");
                }

            }
            catch (Exception e)
            {
                Log.Error("Error during Export data to Power BI. Error: {0}", e.Message);
            }
        }

        public static void SendFacet(Facet facet, string interactionId)
        {
            try
            {
                if (facet is IpInfo)
                {
                    var _ipFacet = facet as IpInfo;

                    //Faking GEO IP data for local environment for testing purposes
                    var geoIpData = FakeGeoIpService.GetGeoIp(_ipFacet.IpAddress);

                    var ipFacet = new Models.IpFacet()
                    {
                        InteractionId = interactionId,
                        AreaCode = _ipFacet.AreaCode,
                        BusinessName = _ipFacet.BusinessName,
                        City = geoIpData.City,
                        Country = geoIpData.Country,
                        Dns = _ipFacet.Dns,
                        IpAddress = geoIpData.IpAddress,
                        Isp = _ipFacet.Isp,
                        Latitude = geoIpData.Latitude,
                        LocationId = _ipFacet.LocationId?.ToString(),
                        Longitude = geoIpData.Longitude,
                        MetroCode = _ipFacet.MetroCode,
                        PostalCode = _ipFacet.PostalCode,
                        Region = _ipFacet.Region,
                        Url = _ipFacet.Url
                    };

                    using (var adapter = new PowerBIAdapter())
                    {
                        adapter.PushRow(ipFacet, "IpFacet");
                    }
                }

                //if (facet is ProfileScores)
                if (facet is Sitecore.XConnect.Collection.Model.ContactBehaviorProfile)
                {
                    var _profileScoresFacet = facet as Sitecore.XConnect.Collection.Model.ContactBehaviorProfile;
                    var profieScoreList = new List<Models.ProfileScoresFacet>();

                    foreach (var score in _profileScoresFacet.Scores)
                    {
                        foreach (var value in score.Value.Values)
                        {
                            profieScoreList.Add(new Models.ProfileScoresFacet()
                            {
                                InteractionId = interactionId,
                                ScoreId = score.Key.ToString(),
                                MatchedPatternId = score.Value.MatchedPatternId?.ToString(),
                                ProfileDefinitionId = score.Value.ProfileDefinitionId.ToString(),
                                Score = score.Value.Score,
                                ScoreCount = score.Value.ScoreCount,
                                ValueId = value.Key.ToString(),
                                Value = value.Value
                            });
                        }
                    }

                    using (var adapter = new PowerBIAdapter())
                    {
                        adapter.PushRows(profieScoreList, "ProfileScoresFacet");
                    }
                }

                if (facet is UserAgentInfo)
                {
                    var _userAgentFacet = facet as UserAgentInfo;
                    var userAgentFacet = new Models.UserAgentFacet()
                    {

                        InteractionId = interactionId,
                        DeviceType = _userAgentFacet.DeviceType,
                        DeviceVendor = _userAgentFacet.DeviceVendor,
                        DeviceVendorHardwareModel = _userAgentFacet.DeviceVendorHardwareModel
                    };

                    using (var adapter = new PowerBIAdapter())
                    {
                        adapter.PushRow(userAgentFacet, "UserAgentFacet");
                    }
                }

                if (facet is WebVisit)
                {
                    var _webVisitFacet = facet as WebVisit;
                    var webVisitFacet = new Models.WebVisitFacet()
                    {
                        InteractionId = interactionId,
                        BrowserMajorName = _webVisitFacet.Browser.BrowserMajorName,
                        BrowserMinorName = _webVisitFacet.Browser.BrowserMinorName,
                        BrowserVersion = _webVisitFacet.Browser.BrowserVersion,
                        Language = _webVisitFacet.Language,
                        MajorVersion = _webVisitFacet.OperatingSystem.MajorVersion,
                        MinorVersion = _webVisitFacet.OperatingSystem.MinorVersion,
                        Name = _webVisitFacet.OperatingSystem.Name,
                        Referrer = _webVisitFacet.Referrer,
#pragma warning disable CS0618 // Type or member is obsolete
                        ReferringSite = _webVisitFacet.ReferringSite,
#pragma warning restore CS0618 // Type or member is obsolete
                        ScreenHeight = _webVisitFacet.Screen.ScreenHeight,
                        ScreenWidth = _webVisitFacet.Screen.ScreenWidth,
                        SearchKeywords = _webVisitFacet.SearchKeywords,
                        SiteName = _webVisitFacet.SiteName
                    };

                    using (var adapter = new PowerBIAdapter())
                    {
                        adapter.PushRow(webVisitFacet, "WebVisitFacet");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error during Export data to Power BI. Error: {0}", e.Message);
            }
        }
    }
}
