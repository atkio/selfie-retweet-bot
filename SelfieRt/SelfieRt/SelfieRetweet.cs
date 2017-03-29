﻿using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SelfieRt
{
    class SelfieRetweet
    {

        public static void Run()
        {


            var db = new SelfieBotDB();

            foreach (ulong id in db.getWaitRetweet())
            {
                reTweet(id).Wait();
                db.removeRetweet(id);
                Thread.Sleep(90 * 1000);
            }

        }


        static SelfieBotConfig config = SelfieBotConfig.Instance;
        private static SingleUserAuthorizer auth = null;

        static void prepare()
        {
            try
            {
                auth = new SingleUserAuthorizer
                {
                    CredentialStore = new SingleUserInMemoryCredentialStore
                    {
                        ConsumerKey = config.ConsumerKey,
                        ConsumerSecret = config.ConsumerSecret,
                        AccessToken = config.AccessToken,
                        AccessTokenSecret = config.AccessTokenSecret
                    }
                };
                auth.AuthorizeAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static async void post(string st)
        {
            if (auth == null)
                prepare();


            var twitterContext = new TwitterContext(auth);
            var tweet = await twitterContext.TweetAsync(st);


        }

        public static async Task reTweet(ulong tweetID)
        {
            if (auth == null)
                prepare();


            var twitterContext = new TwitterContext(auth);
            try
            {
                var retweet = await twitterContext.RetweetAsync(tweetID);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
