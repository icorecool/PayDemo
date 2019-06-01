using PayDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace PayDemo.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }


        #region qq扫码通知，回调接口地址填写：http://localhost:53756/Home/QQCallback
        public ActionResult QQCallback()
        {
            //核对秘钥
            if (Request["key"] != System.Configuration.ConfigurationManager.AppSettings["payKey"])
                return Content("秘钥错误");


            ///↓↓↓↓↓↓↓↓↓提交过来的参数↓↓↓↓↓↓↓↓↓↓↓↓↓↓

            string tradeNo = Request["listid"];//交易号
            string desc = Request["memo"];//支付说明
            string amount = Request["amount"];//金额

            decimal Amount = decimal.Parse(amount) / 100;

            //↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

            string ret = null;

            //充值
            //排除重复订单号
            var o = db.QROrders.FirstOrDefault(t => t.TransferId == tradeNo);
            if (o == null)
            {
                var orderId = long.Parse(desc);
                o = db.QROrders.FirstOrDefault(t => t.Id == orderId);

                if (o != null)
                {
                    var u = db.Users.FirstOrDefault(t => t.UserName == o.UserName);
                    var now = DateTime.Now;

                    if (u != null)
                    {
                        u.Money += Amount;
                        //记录日志，自己加

                        o.UpdateTime = now;
                        o.TransferId = tradeNo;

                        db.SaveChanges();

                        ret = $"#充值成功，当前余额：{u.Money:C}";
                    }
                    else
                        ret = "#无效用户";

                    var hub = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    hub.Clients.Client(o.ConnectionId).callback(ret);
                }
                else
                    ret = "#无效订单号";
            }
            else
                ret = "#重复单号";

            return Content(ret);
        }
        #endregion

        #region 微信扫码通知，回调接口地址填写：http://localhost:53756/Home/MicroChatCallback
        public ActionResult MicroChatCallback()
        {
            //核对秘钥
            if (Request["key"] != System.Configuration.ConfigurationManager.AppSettings["payKey"])
                return Content("秘钥错误");

            var ret = "#充值失败";

            if (Request["amount"] != null)
            {
                string tradeNo = Request["Outtradeno"] ?? Request["time"];//交易号
                string desc = Request["memo"];//支付说明
                string amount = Request["amount"];//金额

                decimal Amount = 0;
                if (decimal.TryParse(amount, out Amount) && Amount > 0)
                {
                    Amount = Amount / 100m;
                }
                else
                    return Content("#金额错误");

                //充值
                //排除重复订单号
                var o = db.QROrders.FirstOrDefault(t => t.TransferId == tradeNo);
                if (o == null)
                {
                    var qrId = long.Parse(desc);
                    var qr = db.WxQRCaches.FirstOrDefault(t => t.Id == qrId);
                    if (qr != null && qr.OrderId > 0)
                    {
                        var orderId = qr.OrderId;
                        qr.OrderId = 0;
                        qr.LockTime = new DateTime(2000, 1, 1);

                        o = db.QROrders.FirstOrDefault(t => t.Id == orderId);

                        if (o != null)
                        {
                            var u = db.Users.FirstOrDefault(t => t.UserName == o.UserName);
                            var now = DateTime.Now;

                            if (u != null)
                            {
                                u.Money += Amount;

                                o.UpdateTime = now;
                                o.TransferId = tradeNo;

                                db.SaveChanges();

                                ret = $"#充值成功，当前余额：{u.Money:C}";
                            }
                            else
                                ret = "#充值失败";

                            var hub = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                            hub.Clients.Client(o.ConnectionId).callback(ret);
                        }
                        else
                            ret = "#无效订单";
                    }
                    else
                        ret = "#无效二维码";

                }
                else
                    ret = "#重复单号";
            }

            return Content(ret);
        }
        #endregion

        #region 支付宝扫码通知，回调接口地址填写：http://localhost:53756/Home/AlipayCallback
        public ActionResult AlipayCallback()
        {
            //核对秘钥
            if (Request["key"] != System.Configuration.ConfigurationManager.AppSettings["payKey"])
                return Content("秘钥错误");


            ///↓↓↓↓↓↓↓↓↓提交过来的参数↓↓↓↓↓↓↓↓↓↓↓↓↓↓

            string tradeNo = Request["tradeNo"];//交易号
            string desc = Request["desc"];//支付说明
            string amount = Request["amount"];//金额

            decimal Amount = decimal.Parse(amount);

            //↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

            string ret = null;

            //充值
            //排除重复订单号
            var o = db.QROrders.FirstOrDefault(t => t.TransferId == tradeNo);
            if (o == null)
            {
                var orderId = long.Parse(desc);
                o = db.QROrders.FirstOrDefault(t => t.Id == orderId);

                if (o != null)
                {
                    var u = db.Users.FirstOrDefault(t => t.UserName == o.UserName);
                    var now = DateTime.Now;

                    if (u != null)
                    {
                        u.Money += Amount;

                        o.UpdateTime = now;
                        o.TransferId = tradeNo;

                        db.SaveChanges();

                        ret = $"#充值成功，当前余额：{u.Money:C}";
                    }
                    else
                        ret = "#充值失败";

                    var hub = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    hub.Clients.Client(o.ConnectionId).callback(ret);
                }
                else
                    ret = "#无效订单";
            }
            else
                ret = "#重复单号";

            return Content(ret);
        }
        #endregion
    }
}