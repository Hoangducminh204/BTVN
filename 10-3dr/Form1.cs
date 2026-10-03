using System.Drawing;
using System.Text.Json;
using QuanLySinhVien.Data;
using QuanLySinhVien.Models;

namespace QuanLySinhVien;

public partial class Form1 : Form
{
    private readonly TextBox _txtMaSV = new();
    private readonly TextBox _txtHoTen = new();
    private readonly ComboBox _cboGioiTinh = new();
    private readonly DateTimePicker _dtpNgaySinh = new();
    private readonly ComboBox _cboLopHoc = new();
    private readonly TextBox _txtDienThoai = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtDiaChi = new();
    private readonly DataGridView _dgvSinhVien = new();
    private readonly Button _btnNhap = new();
    private readonly Button _btnSua = new();
    private readonly Button _btnXoa = new();
    private readonly Button _btnLamMoi = new();
    private readonly BindingSource _studentSource = new();

    private StudentRepository? _repository;
    private bool _updatingForm;

    public Form1()
    {
        Text = "Quản lý sinh viên";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 650);
        Size = new Size(1180, 760);
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.FromArgb(244, 247, 251);

        BuildInterface();
        Load += LoadData;
        Shown += (_, _) => _txtMaSV.Focus();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18),
            BackColor = BackColor
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        Controls.Add(root);

        var heading = new Label
        {
            Text = "QUẢN LÝ SINH VIÊN",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 78, 121),
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(heading, 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = BackColor
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        root.Controls.Add(content, 0, 1);

        content.Controls.Add(BuildStudentEntryPanel(), 0, 0);
        content.Controls.Add(BuildStudentListPanel(), 1, 0);

        var footer = new Label
        {
            Text = "Dữ liệu được lưu cục bộ trên máy tính.",
            Dock = DockStyle.Fill,
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(footer, 0, 2);

        _txtMaSV.TextChanged += (_, _) => RefreshStudentById();
        _dgvSinhVien.CellClick += SelectStudentFromGrid;
        _btnNhap.Click += (_, _) => AddStudent();
        _btnSua.Click += (_, _) => UpdateStudent();
        _btnXoa.Click += (_, _) => DeleteStudent();
        _btnLamMoi.Click += (_, _) => ClearForm();
        SetActionButtons(hasStudentId: false, studentExists: false);
    }

    private Control BuildStudentEntryPanel()
    {
        var group = new GroupBox
        {
            Text = "Thông tin sinh viên",
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 12, 0),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(45, 55, 72)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 9,
            Padding = new Padding(4),
            BackColor = Color.White
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 69));
        for (var row = 0; row < 7; row++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        ConfigureInput(_txtMaSV);
        ConfigureInput(_txtHoTen);
        ConfigureInput(_txtDienThoai);
        ConfigureInput(_txtEmail);
        ConfigureInput(_txtDiaChi);
        _txtMaSV.CharacterCasing = CharacterCasing.Upper;
        _txtMaSV.TabIndex = 0;
        _txtHoTen.TabIndex = 1;
        _cboGioiTinh.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboGioiTinh.Items.AddRange(["Nam", "Nữ", "Khác"]);
        ConfigureInput(_cboGioiTinh);
        _cboGioiTinh.TabIndex = 2;
        _dtpNgaySinh.Format = DateTimePickerFormat.Custom;
        _dtpNgaySinh.CustomFormat = "dd/MM/yyyy";
        _dtpNgaySinh.MaxDate = DateTime.Today;
        _dtpNgaySinh.Value = DateTime.Today.AddYears(-18);
        ConfigureInput(_dtpNgaySinh);
        _dtpNgaySinh.TabIndex = 3;
        _cboLopHoc.DropDownStyle = ComboBoxStyle.DropDownList;
        ConfigureInput(_cboLopHoc);
        _cboLopHoc.TabIndex = 4;
        _txtDienThoai.TabIndex = 5;
        _txtEmail.TabIndex = 6;
        _txtDiaChi.Multiline = true;
        _txtDiaChi.ScrollBars = ScrollBars.Vertical;
        _txtDiaChi.TabIndex = 7;

        AddField(layout, 0, "Mã sinh viên", _txtMaSV);
        AddField(layout, 1, "Họ và tên", _txtHoTen);
        AddField(layout, 2, "Giới tính", _cboGioiTinh);
        AddField(layout, 3, "Ngày sinh", _dtpNgaySinh);
        AddField(layout, 4, "Lớp học", _cboLopHoc);
        AddField(layout, 5, "Điện thoại", _txtDienThoai);
        AddField(layout, 6, "Email", _txtEmail);
        AddField(layout, 7, "Địa chỉ", _txtDiaChi);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Color.White
        };
        ConfigureButton(_btnNhap, "Nhập", Color.FromArgb(37, 99, 235), 8);
        ConfigureButton(_btnSua, "Sửa", Color.FromArgb(5, 150, 105), 9);
        ConfigureButton(_btnXoa, "Xóa", Color.FromArgb(220, 38, 38), 10);
        ConfigureButton(_btnLamMoi, "Làm mới", Color.FromArgb(100, 116, 139), 11);
        buttons.Controls.AddRange([_btnNhap, _btnSua, _btnXoa, _btnLamMoi]);
        layout.Controls.Add(buttons, 0, 8);
        layout.SetColumnSpan(buttons, 2);
        group.Controls.Add(layout);
        return group;
    }

    private Control BuildStudentListPanel()
    {
        var group = new GroupBox
        {
            Text = "Danh sách sinh viên",
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(45, 55, 72)
        };
        _dgvSinhVien.Dock = DockStyle.Fill;
        _dgvSinhVien.BackgroundColor = Color.White;
        _dgvSinhVien.BorderStyle = BorderStyle.None;
        _dgvSinhVien.AllowUserToAddRows = false;
        _dgvSinhVien.AllowUserToDeleteRows = false;
        _dgvSinhVien.ReadOnly = true;
        _dgvSinhVien.MultiSelect = false;
        _dgvSinhVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _dgvSinhVien.AutoGenerateColumns = false;
        _dgvSinhVien.RowHeadersVisible = false;
        _dgvSinhVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _dgvSinhVien.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 232, 240);
        _dgvSinhVien.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        _dgvSinhVien.EnableHeadersVisualStyles = false;
        _dgvSinhVien.Columns.AddRange(
        [
            CreateGridColumn("Mã SV", nameof(SinhVien.MaSV), 17),
            CreateGridColumn("Họ và tên", nameof(SinhVien.HoTen), 28),
            CreateGridColumn("Giới tính", nameof(SinhVien.GioiTinh), 13),
            CreateGridColumn("Ngày sinh", nameof(SinhVien.NgaySinh), 17, "dd/MM/yyyy"),
            CreateGridColumn("Mã lớp", nameof(SinhVien.MaLop), 15)
        ]);
        group.Controls.Add(_dgvSinhVien);
        return group;
    }

    private void LoadData(object? sender, EventArgs e)
    {
        try
        {
            _repository = new StudentRepository();
        }
        catch (Exception exception) when (
            exception is IOException or JsonException or UnauthorizedAccessException or ArgumentException)
        {
            MessageBox.Show(
                this,
                $"Không thể tải dữ liệu sinh viên: {exception.Message}",
                "Lỗi dữ liệu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            Close();
            return;
        }

        _cboLopHoc.DataSource = _repository.LopHocs;
        _cboLopHoc.DisplayMember = nameof(LopHoc.DisplayName);
        _cboLopHoc.ValueMember = nameof(LopHoc.MaLop);
        _studentSource.DataSource = _repository.SinhViens;
        _dgvSinhVien.DataSource = _studentSource;
        ClearForm();
    }

    private void RefreshStudentById()
    {
        if (_updatingForm || _repository is null)
        {
            return;
        }

        var studentId = _txtMaSV.Text.Trim();
        var student = _repository.SinhViens.FirstOrDefault(
            item => string.Equals(item.MaSV, studentId, StringComparison.OrdinalIgnoreCase));
        if (student is null)
        {
            ClearStudentFields();
            SetInputEnabled(true);
            SetActionButtons(hasStudentId: studentId.Length > 0, studentExists: false);
            return;
        }

        _updatingForm = true;
        _txtHoTen.Text = student.HoTen;
        _cboGioiTinh.SelectedItem = student.GioiTinh;
        _dtpNgaySinh.Value = student.NgaySinh;
        _cboLopHoc.SelectedValue = student.MaLop;
        _txtDienThoai.Text = student.DienThoai ?? string.Empty;
        _txtEmail.Text = student.Email ?? string.Empty;
        _txtDiaChi.Text = student.DiaChi ?? string.Empty;
        _updatingForm = false;
        SetInputEnabled(true);
        SetActionButtons(hasStudentId: true, studentExists: true);
    }

    private void AddStudent()
    {
        if (_repository is null)
        {
            return;
        }

        var student = CreateStudentFromForm();
        if (!ValidateStudent(student))
        {
            return;
        }

        try
        {
            _repository.AddStudent(student);
            RefreshStudentGrid();
            RefreshStudentById();
            MessageBox.Show(this, "Đã thêm sinh viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            ShowSaveError(exception);
        }
    }

    private void UpdateStudent()
    {
        if (_repository is null)
        {
            return;
        }

        var student = CreateStudentFromForm();
        if (!ValidateStudent(student))
        {
            return;
        }

        try
        {
            _repository.UpdateStudent(student);
            RefreshStudentGrid();
            RefreshStudentById();
            MessageBox.Show(this, "Đã cập nhật sinh viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            ShowSaveError(exception);
        }
    }

    private void DeleteStudent()
    {
        if (_repository is null || string.IsNullOrWhiteSpace(_txtMaSV.Text))
        {
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            $"Bạn có chắc muốn xóa sinh viên '{_txtMaSV.Text.Trim()}' không?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _repository.DeleteStudent(_txtMaSV.Text.Trim());
            RefreshStudentGrid();
            ClearForm();
            MessageBox.Show(this, "Đã xóa sinh viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            ShowSaveError(exception);
        }
    }

    private SinhVien CreateStudentFromForm()
    {
        return new SinhVien
        {
            MaSV = _txtMaSV.Text.Trim(),
            HoTen = _txtHoTen.Text.Trim(),
            GioiTinh = _cboGioiTinh.SelectedItem?.ToString() ?? string.Empty,
            NgaySinh = _dtpNgaySinh.Value.Date,
            MaLop = _cboLopHoc.SelectedValue?.ToString() ?? string.Empty,
            DienThoai = NullIfEmpty(_txtDienThoai.Text),
            Email = NullIfEmpty(_txtEmail.Text),
            DiaChi = NullIfEmpty(_txtDiaChi.Text)
        };
    }

    private bool ValidateStudent(SinhVien student)
    {
        if (student.IsValid(out var errors))
        {
            return true;
        }

        MessageBox.Show(
            this,
            string.Join(Environment.NewLine, errors),
            "Thông tin chưa hợp lệ",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        return false;
    }

    private void SelectStudentFromGrid(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _dgvSinhVien.Rows[e.RowIndex].DataBoundItem is not SinhVien student)
        {
            return;
        }

        _txtMaSV.Text = student.MaSV;
        _txtMaSV.Focus();
    }

    private void RefreshStudentGrid()
    {
        if (_repository is null)
        {
            return;
        }

        _studentSource.DataSource = _repository.SinhViens;
        _studentSource.ResetBindings(false);
    }

    private void ClearForm()
    {
        _updatingForm = true;
        _txtMaSV.Clear();
        _updatingForm = false;
        ClearStudentFields();
        SetInputEnabled(true);
        SetActionButtons(hasStudentId: false, studentExists: false);
        _txtMaSV.Focus();
    }

    private void ClearStudentFields()
    {
        _txtHoTen.Clear();
        _cboGioiTinh.SelectedIndex = -1;
        _dtpNgaySinh.Value = DateTime.Today.AddYears(-18);
        _cboLopHoc.SelectedIndex = -1;
        _txtDienThoai.Clear();
        _txtEmail.Clear();
        _txtDiaChi.Clear();
    }

    private void SetInputEnabled(bool enabled)
    {
        _txtHoTen.Enabled = enabled;
        _cboGioiTinh.Enabled = enabled;
        _dtpNgaySinh.Enabled = enabled;
        _cboLopHoc.Enabled = enabled;
        _txtDienThoai.Enabled = enabled;
        _txtEmail.Enabled = enabled;
        _txtDiaChi.Enabled = enabled;
    }

    private void SetActionButtons(bool hasStudentId, bool studentExists)
    {
        _btnNhap.Enabled = hasStudentId && !studentExists;
        _btnSua.Enabled = studentExists;
        _btnXoa.Enabled = studentExists;
        _btnLamMoi.Enabled = true;
    }

    private void ShowSaveError(Exception exception)
    {
        MessageBox.Show(
            this,
            $"Không thể lưu thay đổi: {exception.Message}",
            "Lỗi lưu dữ liệu",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private static string? NullIfEmpty(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static void AddField(TableLayoutPanel layout, int row, string labelText, Control input)
    {
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(51, 65, 85),
            TabStop = false
        };
        layout.Controls.Add(label, 0, row);
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(3, 6, 3, 6);
        layout.Controls.Add(input, 1, row);
    }

    private static void ConfigureInput(Control control)
    {
        control.Font = new Font("Segoe UI", 10);
        control.BackColor = Color.White;
    }

    private static void ConfigureButton(Button button, string text, Color color, int tabIndex)
    {
        button.Text = text;
        button.Width = 88;
        button.Height = 38;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        button.Margin = new Padding(3, 3, 6, 3);
        button.TabIndex = tabIndex;
        button.Cursor = Cursors.Hand;
    }

    private static DataGridViewTextBoxColumn CreateGridColumn(
        string header,
        string property,
        float fillWeight,
        string? format = null)
    {
        return new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = property,
            FillWeight = fillWeight,
            DefaultCellStyle = { Format = format ?? string.Empty }
        };
    }
}
