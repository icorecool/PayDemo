using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace PayDemo.Models
{
    [Authorize]
    public class ChatHub : Hub
    {
        ApplicationDbContext db = new ApplicationDbContext();


        public string GetQR(string type, decimal amount)
        {
            try
            {
                var time = DateTime.Now.AddDays(-1);
                var UserName = this.Context.User.Identity.Name;
                var count = db.QROrders.Count(t => t.UserName == UserName && t.CreateTime > time);
                if (count > 10)
                    return "今日订单过多，明日再来吧";

                var order = new QROrder()
                {
                    Id = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds) * 1000 + new Random().Next(1000),
                    UserName = UserName,
                    Amount = amount,
                    CreateTime = DateTime.Now,
                    PayType = type,
                    ConnectionId = Context.ConnectionId
                };
                db.QROrders.Add(order);
                db.SaveChanges();

                switch (type)
                {
                    case "alipay":
                        return Util.ToQRBase64(string.Format(System.Configuration.ConfigurationManager.AppSettings["alipaypay"], amount, order.Id));
                    case "wechat":
                        {
                            time = DateTime.Now.AddMinutes(-5);
                            var qr = db.WxQRCaches.OrderBy(t => t.LockTime).FirstOrDefault(t => t.Content != null && t.Amount == amount && t.LockTime < time);

                            if (qr == null)//生成新的
                            {
                                qr = new WxQRCache()
                                {
                                    Amount = amount,
                                    LockTime = DateTime.Now,
                                    OrderId = order.Id
                                };
                                db.WxQRCaches.Add(qr);
                                db.SaveChanges();//得到id

                                string url = string.Format(System.Configuration.ConfigurationManager.AppSettings["wechatpay"], (int)(qr.Amount * 100), qr.Id, System.Configuration.ConfigurationManager.AppSettings["payKey"]);

                                var html = new WebClient().DownloadString(url);
                                if (!string.IsNullOrWhiteSpace(html) && html.StartsWith("wxp://"))
                                {
                                    qr.Content = html;
                                }
                                else
                                    return "通道故障，请更换支付方式";
                            }
                            else//重复利用
                            {
                                qr.OrderId = order.Id;
                                qr.LockTime = DateTime.Now;
                            }

                            db.SaveChanges();
                            if (!string.IsNullOrEmpty(qr.Content))
                            {
                                return Util.ToQRBase64(qr.Content);
                            }
                        }
                        break;
                    case "qq":
                        {
                            //这里可以参考微信，做二维码缓存

                            string url = string.Format(System.Configuration.ConfigurationManager.AppSettings["qqpay"], (int)(amount * 100), order.Id, System.Configuration.ConfigurationManager.AppSettings["payKey"]);
                            var html = new WebClient().DownloadString(url);

                            if (!string.IsNullOrEmpty(html) && html.StartsWith("http"))
                            {
                                return Util.ToQRBase64(html);
                            }
                            else
                                return "通道故障，请更换支付方式";
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                //记录日志
            }

            return "通道故障，请更换支付方式或重试";
        }
    }
}