using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using QLTV.Models;

namespace QLTV
{
    public partial class AWRatingWindow : Window
    {
        private QLTVContext _context = new QLTVContext();
        private int _docGiaId; 

        public AWRatingWindow(int docGiaId)
        {
            InitializeComponent();
            _docGiaId = docGiaId;
            LoadBooksData();
        }

        private void LoadBooksData()
        {
            // Lấy danh sách sách đã mượn bởi độc giả hiện tại
            var books = _context.CTPHIEUMUON
                .Include(ct => ct.IDSachNavigation)
                .ThenInclude(s => s.IDTuaSachNavigation)
                .Include(ct => ct.IDPhieuMuonNavigation)
                .Where(ct => ct.IDPhieuMuonNavigation.IDDocGia == _docGiaId)
                .ToList();

            // Tạo danh sách đánh giá 
            foreach (var book in books)
            {
                if (!_context.DANHGIA.Any(dg => dg.IDSach == book.IDSach && dg.IDPhieuMuon == book.IDPhieuMuon))
                {
                    _context.DANHGIA.Add(new DANHGIA { IDSach = book.IDSach, IDPhieuMuon = book.IDPhieuMuon, DanhGia = 0 });
                }
            }
            _context.SaveChanges();

            // Lấy database
            var ratings = new ObservableCollection<DANHGIA>(
                _context.DANHGIA
                    .Include(dg => dg.IDSachNavigation)
                    .ThenInclude(s => s.IDTuaSachNavigation)
                    .Include(dg => dg.IDPhieuMuonNavigation)
                    .Where(dg => dg.IDPhieuMuonNavigation.IDDocGia == _docGiaId)
                    .ToList()
            );

            BooksDataGrid.ItemsSource = ratings;
        }

        private void SaveRating_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in BooksDataGrid.Items)
                {
                    var rating = item as DANHGIA;
                    if (rating != null)
                    {
                        // Tìm đối bằng ID
                        var existingRating = _context.DANHGIA.FirstOrDefault(d => d.ID == rating.ID);

                        if (existingRating != null)
                        {
                            existingRating.DanhGia = rating.DanhGia;
                            _context.Entry(existingRating).State = EntityState.Modified;
                        }
                    }
                }

                _context.SaveChanges();
                BooksDataGrid.Items.Refresh();

                MessageBox.Show("Đánh giá đã được lưu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu đánh giá: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}