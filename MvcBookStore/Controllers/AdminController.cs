using MvcBookStore.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;


namespace MvcBookStore.Controllers
{

    public class AdminController : Controller
    {
        QLBANSACHEntities db = new QLBANSACHEntities();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sach(int? page)
        {
            int pageSize = 5;
            int pageNum = (page ?? 1);  // Nếu không có trang, mặc định là trang 1

            var sachList = db.SACHes.ToList();  // Lấy danh sách sách từ cơ sở dữ liệu
            var pagedSachList = sachList.ToPagedList(pageNum, pageSize);  // Phân trang dữ liệu

            return View(pagedSachList);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var tendn = collection["username"];
            var matkhau = collection["password"];
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                Admin ad = db.Admins.SingleOrDefault(n => n.UserAdmin == tendn && n.PassAdmin == matkhau);
                if (ad != null)
                {
                    Session["Taikhoanadmin"] = ad;
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult ThemmoiSach()
        {
            // Đưa dữ liệu vào dropdown list
            // lấy ds từ table chủ đề, sx tăng dần theo tên chủ đề, chọn lấy gtri Ma CD, hiện thị tên chủ đề
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChude");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemmoiSach(SACH sach, HttpPostedFileBase fileUpload)
        {
            // Đưa dữ liệu vào dropdownload
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChude");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

            // Kiểm tra đường dẫn file
            if (fileUpload == null)
            {
                ViewBag.Thongbao = "Vui lòng chọn ảnh bìa";
                return View();
            }
            // thêm vào CSDL
            else
            {
                if (ModelState.IsValid)
                {
                    // lưu tên file, lưu ý bổ sung thư viện using System.IO;
                    var fileName = Path.GetFileName(fileUpload.FileName);

                    // lưu đường dẫn của file
                    var path = Path.Combine(Server.MapPath("~/Content/image/"), fileName);

                    // ktr hình ảnh tồn tại chưa
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Thongbao = "Hình ảnh đã tồn tại";

                    }
                    else
                    {
                        // lưu ảnh vào đường dẫn
                        fileUpload.SaveAs(path);
                    }
                    sach.Anhbia = fileName;
                    // lưu vào CSDL
                    db.SACHes.Add(sach);
                    db.SaveChanges();
                }
                return RedirectToAction("Sach");
            }
        }

        // Hiển thị sản phẩm
        public ActionResult Chitietsach(int id)
        {
            // Lấy ra đối tượng sách theo mã
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sach);
        }

        // Xóa sp
        [HttpGet]
        public ActionResult Xoasach(int id)
        {
            // lấy ra đối tượng cần xóa theo mã
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sach);
        }

        [HttpPost, ActionName("Xoasach")]
        public ActionResult Xacnhanxoa(int id)
        {
            // lấy ra đối tượng cần xóa theo mã
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.SACHes.Remove(sach);
            db.SaveChanges();
            return RedirectToAction("sach");
        }

        // CHỉnh sửa sách
        //[HttpGet]
        //public ActionResult Suasach(int id)
        //{
        //    SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
        //    if (sach == null)
        //    {
        //        Response.StatusCode = 404;
        //        return null;
        //    }

        //    // Đưa dữ liệu vào dropdown list
        //    // lấy ds từ table chủ đề, sx tăng dần theo tên chủ đề, chọn lấy gtri Ma CD, hiện thị tên chủ đề
        //    ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChude", sach.MaCD);
        //    ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);
        //    return View(sach);
        //}

        //[HttpPost]
        //[ValidateInput(false)]
        //public ActionResult Suasach(SACH sach, HttpPostedFileBase fileUpload)
        //{
        //    // Đưa dữ liệu vào dropdownload
        //    ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChude");
        //    ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

        //    if (fileUpload == null)
        //    {
        //        ViewBag.Thongbao = "Vui lòng chọn ảnh bìa";
        //        return View();
        //    }
        //    else
        //    {

        //        if (ModelState.IsValid)
        //        {
        //            var fileName = Path.GetFileName(fileUpload.FileName);
        //            var path = Path.Combine(Server.MapPath("~/Content/image"), fileName);
        //            if (System.IO.File.Exists(path))
        //            {
        //                ViewBag.Thongbao = "Hình ảnh đã tồn tại";
        //            }
        //            else
        //            {
        //                fileUpload.SaveAs(path);
        //            }
        //            sach.Anhbia = fileName;
        //            UpdateModel(sach);
        //            db.SaveChanges();
        //        }
        //    }

        //    return RedirectToAction("Sach");
        //}

        // Chỉnh sửa sách
        // Chỉnh sửa sách
        [HttpGet]
        public ActionResult Suasach(int id)
        {
            // Kiểm tra giá trị id
            if (id == 0)
            {
                Response.StatusCode = 400;
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            if (sach == null)
            {
                Response.StatusCode = 404;
                return HttpNotFound();
            }

            // Đưa dữ liệu vào dropdown list
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);
            return View(sach);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Suasach(SACH sach, HttpPostedFileBase fileUpload)
        {
            // Đưa dữ liệu vào dropdown list
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/image"), fileName);
                if (System.IO.File.Exists(path))
                {
                    ViewBag.Thongbao = "Hình ảnh đã tồn tại";
                    return View(sach);
                }
                else
                {
                    fileUpload.SaveAs(path);
                    sach.Anhbia = fileName;
                }
            }
            else
            {
                var oldSach = db.SACHes.AsNoTracking().SingleOrDefault(n => n.Masach == sach.Masach);
                if (oldSach != null)
                {
                    sach.Anhbia = oldSach.Anhbia;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(sach).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Sach");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (SACH)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Không thể lưu. Lỗi!");
                    }
                    else
                    {
                        var databaseValues = (SACH)databaseEntry.ToObject();

                        if (databaseValues.Tensach != clientValues.Tensach)
                            ModelState.AddModelError("Tensach", $"Giá trị hiện tại: {databaseValues.Tensach}");
                        if (databaseValues.Giaban != clientValues.Giaban)
                            ModelState.AddModelError("Giaban", $"Giá trị hiện tại: {databaseValues.Giaban}");
                        if (databaseValues.Mota != clientValues.Mota)
                            ModelState.AddModelError("Mota", $"Giá trị hiện tại: {databaseValues.Mota}");
                        if (databaseValues.Anhbia != clientValues.Anhbia)
                            ModelState.AddModelError("Anhbia", $"Giá trị hiện tại: {databaseValues.Anhbia}");
                        if (databaseValues.Ngaycapnhat != clientValues.Ngaycapnhat)
                            ModelState.AddModelError("Ngaycapnhat", $"Giá trị hiện tại: {databaseValues.Ngaycapnhat}");
                        if (databaseValues.Soluongton != clientValues.Soluongton)
                            ModelState.AddModelError("Soluongton", $"Giá trị hiện tại: {databaseValues.Soluongton}");
                        if (databaseValues.MaCD != clientValues.MaCD)
                            ModelState.AddModelError("MaCD", $"Giá trị hiện tại: {databaseValues.MaCD}");
                        if (databaseValues.MaNXB != clientValues.MaNXB)
                            ModelState.AddModelError("MaNXB", $"Giá trị hiện tại: {databaseValues.MaNXB}");

                        ModelState.AddModelError(string.Empty, "vui lòng kiểm tra và thử lại.");

                        db.Entry(sach).OriginalValues.SetValues(databaseValues);
                    }
                }
            }

            return View(sach);
        }

        public ActionResult ThongKeSach()
        {
            // Dữ liệu từ database
            //var labels = db.CHUDEs.Select(c => c.TenChuDe).ToArray();
            //var values = new[] { 617594, 181045, 153060, 106519, 105162 };

            // Chuyển đổi sang JSON
            //ViewBag.ChartLabels = JsonConvert.SerializeObject(labels);
            //ViewBag.ChartValues = JsonConvert.SerializeObject(values);

            //return View();


            var booksByCategory = db.SACHes
.GroupBy(s => s.CHUDE.TenChuDe)  // Assuming `Tenchude` is the category name in CHUDE
.Select(g => new { Category = g.Key, Count = g.Count() })
.ToList();

            ViewBag.ChartLabels = booksByCategory.Select(b => b.Category).ToArray();
            ViewBag.ChartData = booksByCategory.Select(b => b.Count).ToArray();

            return View();
        }

    }
}