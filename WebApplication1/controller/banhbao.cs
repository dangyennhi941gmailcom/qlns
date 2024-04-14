using QLNS.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace QLNS.Controllers
{
    public class BangCongController : Controller
    {
        dbQLNSDataContext db = new dbQLNSDataContext();
        public ActionResult Index(string searchString)
        {
            tb_NHANVIEN nvSession = (tb_NHANVIEN)Session["user"];
            var count = db.PHANQUYENs.Count(m => m.MANV == nvSession.MANV & m.MACHUCNANG == 4);
            if (count == 0)
            {
                return Redirect("https://localhost:44399/BaoLoi/Index");
            }
            ViewBag.KeyWord = searchString;
            var all_bangcong = (from tb_BANGCONG in db.tb_BANGCONGs select tb_BANGCONG).OrderBy(m => m.MACONG);
            if (!string.IsNullOrEmpty(searchString)) all_bangcong = (IOrderedQueryable<tb_BANGCONG>)all_bangcong.Where(a => a.TENNV.Contains(searchString));
            return View(all_bangcong);
        }
        public ActionResult Index_QL(string searchString)
        {
            ViewBag.KeyWord = searchString;
            var all_bangcong = (from tb_BANGCONG in db.tb_BANGCONGs select tb_BANGCONG).OrderBy(m => m.MACONG);
            if (!string.IsNullOrEmpty(searchString)) all_bangcong = (IOrderedQueryable<tb_BANGCONG>)all_bangcong.Where(a => a.TENNV.Contains(searchString));
            return View(all_bangcong);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(FormCollection collection, tb_BANGCONG p)
        {
            var E_tenNV = collection["TENNV"];
            var E_maNV = collection["MANV"];
            var E_tenCV = collection["TENCV"];
            var E_tenPB = collection["TENPB"];
            var E_ngaygiolam = DateTime.Now;
            DateTime dateOnly = E_ngaygiolam.Date;
            if (string.IsNullOrEmpty(E_maNV))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                p.TENNV = E_tenNV.ToString();
                p.TENCV = E_tenCV.ToString();
                p.TENPB = E_tenPB.ToString();
                p.NGAYGIOLAM = DateTime.Now;
                /* p.NGAYGIORA = DateTime.Now;*/
                p.MANV = E_maNV.AsInt();
                db.tb_BANGCONGs.InsertOnSubmit(p);
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            return View(p);
        }
        public ActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Checkout(FormCollection collection, tb_BANGCONG p)
        {
            var E_maNV = collection["MANV"];

            if (string.IsNullOrEmpty(E_maNV))
            {
                ViewData["Error"] = "Don't empty!";
                return View(p);
            }

            int maNVValue; // Khai báo biến maNVValue ở đây

            if (int.TryParse(E_maNV, out maNVValue))
            {
                var latestCheckinEmployee = db.tb_BANGCONGs
                    .Where(x => x.MANV.HasValue && x.MANV.Value == maNVValue && x.NGAYGIOLAM <= DateTime.Now)
                    .OrderByDescending(x => x.NGAYGIOLAM)
                    .FirstOrDefault();

                if (latestCheckinEmployee != null)
                {
                    // Cập nhật thông tin "checkout" của nhân viên
                    latestCheckinEmployee.TENNV = collection["TENNV"];
                    latestCheckinEmployee.TENCV = collection["TENCV"];
                    latestCheckinEmployee.TENPB = collection["TENPB"];
                    latestCheckinEmployee.NGAYGIORA = DateTime.Now;

                    db.SubmitChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    // Xử lý khi không tìm thấy thông tin "checkin" gần nhất của nhân viên
                    ViewData["Error"] = "Employee hasn't checked in!";
                    return View(p);
                }
            }
            else
            {
                // Xử lý khi không thể chuyển đổi E_maNV sang kiểu int
                ViewData["Error"] = "Invalid employee ID!";
                return View(p);
            }
        }
        public ActionResult Edit(int id)
        {
            var E_bangcong = db.tb_BANGCONGs.First(m => m.MACONG == id);
            return View(E_bangcong);
        }
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var E_bangcong = db.tb_BANGCONGs.First(m => m.MACONG == id);
            var E_Tonggiolam = collection["TONGGIOLAM"];
            E_bangcong.MACONG = id;
            if (string.IsNullOrEmpty(E_Tonggiolam))
            {
                ViewData["Error"] = "don't empty";
            }
            else
            {
                E_bangcong.TONGGIOLAM = E_Tonggiolam.AsInt();
                UpdateModel(E_bangcong);
                db.SubmitChanges();
                return RedirectToAction("Index_QL");
            }
            return this.Edit(id);
        }
    }
}