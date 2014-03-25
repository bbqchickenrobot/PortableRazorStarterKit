﻿using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

namespace PortableCongress
{
    public class WebAccess
    {
        const string API_KEY = "609b2b4b92f74ce9ac5c86487146d107";

        public WebAccess ()
        {
        }

        public static async Task<RecentVotes> GetRecentVotesAsync (int id)
        {
            try {
                using (var httpClient = new HttpClient ()) {
                    string url = String.Format ("http://www.govtrack.us/users/events-rss2.xpd?monitors=pv:{0}&days=30", id);

                    var response = await httpClient.GetAsync (url);
                    var stream = await response.Content.ReadAsStreamAsync ();
                    var votes = LoadVotes (stream);
                    var recentVotes = new RecentVotes { Id = id, Votes = votes };

                    return recentVotes;
                }
            } catch (Exception) {
                var recentVotes = new RecentVotes {Id = id, Votes = new List<Vote> {
                        new Vote { Title = "Could not connect to the internet" }
                    }
                };
                return recentVotes;
            }
        }

        public static async Task<Committees> GetCommitteesAsync (int id, string bioGuideId)
        {
            try {
                using (var httpClient = new HttpClient ()) {
                    string url = String.Format ("http://services.sunlightlabs.com/api/committees.allForLegislator.xml?apikey={0}&bioguide_id={1}", API_KEY, bioGuideId);

                    var response = await httpClient.GetAsync (url);
                    var stream = await response.Content.ReadAsStreamAsync ();
                    var committeeList = LoadCommittees (stream);
                    var committees = new Committees { Id = id, CommitteeList = committeeList };

                    return committees;	
                }
            } catch (Exception) {
                var commitees = new Committees {Id = id, CommitteeList = new List<Committee> {
                        new Committee { Name = "Could not connect to the internet" }
                    }
                };
                return commitees;
            }
        }

        static List<Committee> LoadCommittees (Stream stream)
        {
            XDocument committeeData = XDocument.Load (stream);

            var committeeList = (from c in committeeData.Descendants ("committee")
                                 select new Committee { Name = c.Element ("name").Value }).ToList ();

            return committeeList;
        }

        static List<Vote> LoadVotes (Stream stream)
        {
            XDocument voteFeed = XDocument.Load (stream);

            var votes = (from item in voteFeed.Descendants ("item")
                         select new Vote {
                            Title = item.Element ("title").Value,
                            PublicationDate = DateTime.Parse (item.Element ("pubDate").Value),
                            Link = item.Element ("link").Value,
                            Description = item.Element ("description").Value
                        }).OrderByDescending (v => v.PublicationDate).ToList ();

            return votes;
        }
    }
}
