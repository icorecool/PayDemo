using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PayDemo.Models
{
    // 可以通过将更多属性添加到 ApplicationUser 类来为用户添加配置文件数据，请访问 https://go.microsoft.com/fwlink/?LinkID=317594 了解详细信息。
    public class ApplicationUser : IdentityUser
    {
        public decimal Money { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // 请注意，authenticationType 必须与 CookieAuthenticationOptions.AuthenticationType 中定义的相应项匹配
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在此处添加自定义用户声明
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<WxQRCache> WxQRCaches { get; set; }
        public DbSet<QROrder> QROrders { get; set; }
    }
    /// <summary>
    /// 微信二维码缓存（QQ也可以用这个办法，大家自己实现）
    /// </summary>
    [Table("WxQRCache")]
    public class WxQRCache
    {
        /// <summary>
        /// 二维码备注
        /// 需要新的二维码时，先插入一条记录，得到id，
        /// 然后用id作为备注去调用二维码服务器，生成二维码
        /// 付款后可以根据备注找到二维码，根据下面的OrderId找到订单
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 二维码金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 二维码内容，wxp://xxxx
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public long OrderId { get; set; }
        /// <summary>
        /// 锁定二维码时间
        /// </summary>
        public DateTime LockTime { get; set; }
    }
    /// <summary>
    /// 订单表
    /// </summary>
    [Table("QROrder")]
    public class QROrder
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        /// <summary>
        /// 充值的账号（其它业务原理类似，例如买东西的产品id，订单内容等）
        /// </summary>
        public string UserName { get; set; }
        public DateTime CreateTime { get; set; }
        public decimal Amount { get; set; }
        /// <summary>
        /// 交易号，防止重复充值
        /// </summary>
        public string TransferId { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 支付类型
        /// </summary>
        public string PayType { get; set; }
        /// <summary>
        /// signalR使用通知客户端更新界面
        /// </summary>
        public string ConnectionId { get; set; }
    }
}