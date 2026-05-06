// Creator David Kofod Hanna and GitHub Copilot, April 2026
// Based on beginner script from Tabular Editor: https://docs.tabulareditor.com/common/CSharpScripts/Beginner/script-create-field-parameter.html

// Creates a Field Parameter table with an interactive dialog for selecting
// the parameter table name and which columns/measures to include.
// Any objects already selected in the TOM Explorer will be pre-checked.
// Hierarchies and hidden columns or measures are excluded from the selection tree.
// Currently only works in Tabular Editor 3

using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

// IWin32Window wrapper — makes the form appear in front of the TE3 overlay.
class Win32Owner : IWin32Window {
    public Win32Owner(IntPtr h) { Handle = h; }
    public IntPtr Handle { get; }
}
var dialogOwner = new Win32Owner(Process.GetCurrentProcess().MainWindowHandle);

// ── Fluent 2 Design Tokens ────────────────────────────────────────────────
var fntUI     = new Font("Segoe UI", 9f);
var fntUISemi = new Font("Segoe UI", 9f, FontStyle.Bold);
var clrText   = Color.FromArgb(32, 31, 30);
var clrTextSec= Color.FromArgb(96, 94, 92);
var clrBorder = Color.FromArgb(200, 198, 196);

// Owner-drawn button with rounded corners and Fluent color states.
class FluentButton : Button {
    public bool IsPrimary { get; set; }
    bool _hov, _prs;
    const int R = 5;
    public FluentButton() {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
               | ControlStyles.OptimizedDoubleBuffer, true);
        FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand; Height = 32;
        Font = new System.Drawing.Font("Segoe UI", 9f);
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hov = true;          Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hov = _prs = false;  Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); _prs = true;        Invalidate(); }
    protected override void OnMouseUp(MouseEventArgs e)   { base.OnMouseUp(e);   _prs = false;       Invalidate(); }
    protected override void OnPaint(PaintEventArgs e) {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        System.Drawing.Color bg, fg, br;
        if (IsPrimary) {
            bg = _prs ? System.Drawing.Color.FromArgb(0, 90, 158) : _hov ? System.Drawing.Color.FromArgb(16, 110, 190) : System.Drawing.Color.FromArgb(0, 120, 212);
            fg = System.Drawing.Color.White; br = bg;
        } else {
            bg = _prs ? System.Drawing.Color.FromArgb(237, 235, 233) : _hov ? System.Drawing.Color.FromArgb(243, 242, 241) : System.Drawing.Color.White;
            fg = System.Drawing.Color.FromArgb(32, 31, 30); br = System.Drawing.Color.FromArgb(138, 136, 134);
        }
        var rc = new System.Drawing.Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = RR(rc, R)) using (var b = new System.Drawing.SolidBrush(bg)) using (var p = new System.Drawing.Pen(br)) {
            g.FillPath(b, path);
            if (!IsPrimary) g.DrawPath(p, path);
        }
        TextRenderer.DrawText(g, Text, Font, rc, fg,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
    }
    static System.Drawing.Drawing2D.GraphicsPath RR(System.Drawing.Rectangle r, int rad) {
        var gp = new System.Drawing.Drawing2D.GraphicsPath();
        gp.AddArc(r.X,           r.Y,            rad*2, rad*2, 180, 90);
        gp.AddArc(r.Right-rad*2, r.Y,            rad*2, rad*2, 270, 90);
        gp.AddArc(r.Right-rad*2, r.Bottom-rad*2, rad*2, rad*2,   0, 90);
        gp.AddArc(r.X,           r.Bottom-rad*2, rad*2, rad*2,  90, 90);
        gp.CloseFigure(); return gp;
    }
}

// ── Pre-select objects already highlighted in the TOM Explorer ─────────────
var preSelected = new HashSet<ITabularTableObject>(
    Selected.Columns.Cast<ITabularTableObject>()
            .Concat(Selected.Measures.Cast<ITabularTableObject>())
);

// ── Flat item list + per-object checked-state map ─────────────────────────
// Checked state is persisted here so filtering never loses selections.
var allItems = new List<(string TableName, string DisplayName, ITabularTableObject Obj)>();
var checkedState = new Dictionary<ITabularTableObject, bool>();

foreach (var tbl in Model.Tables.OrderBy(t => t.Name))
{
    foreach (var m in tbl.Measures.Where(m => !m.IsHidden).OrderBy(m => m.Name))
    {
        allItems.Add((tbl.Name, "[M]  " + m.Name, (ITabularTableObject)m));
        checkedState[(ITabularTableObject)m] = preSelected.Contains(m);
    }
    foreach (var c in tbl.Columns
                         .Where(c => c.Type != ColumnType.RowNumber && !c.IsHidden)
                         .OrderBy(c => c.Name))
    {
        allItems.Add((tbl.Name, "[C]  " + c.Name, (ITabularTableObject)c));
        checkedState[(ITabularTableObject)c] = preSelected.Contains(c);
    }
}

// ── Build the form ─────────────────────────────────────────────────────────
var form = new Form {
    Text             = "Create Field Parameter",
    Size             = new Size(500, 700),
    StartPosition    = FormStartPosition.CenterScreen,
    FormBorderStyle  = FormBorderStyle.FixedDialog,
    MaximizeBox      = false,
    MinimizeBox      = false,
    BackColor        = Color.White,
    Font             = fntUI
};

// Parameter name row
var lblName = new Label   { Text = "Parameter Table Name:", Location = new Point(12, 12), AutoSize = true,
                            Font = fntUISemi, ForeColor = clrText };
var txtName = new TextBox { Text = "Parameter", Location = new Point(12, 32), Width = 460,
                            Font = fntUI, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };

// Search row
var lblSearch = new Label   { Text = "Search:", Location = new Point(12, 62), AutoSize = true,
                              Font = fntUISemi, ForeColor = clrText };
var txtSearch = new TextBox { Location = new Point(12, 82), Width = 460,
                              Font = fntUI, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };

// Objects header (left) + selection counter (right-aligned)
var lblObjects = new Label {
    Text      = "Select Columns and Measures:",
    Location  = new Point(12, 112),
    AutoSize  = true,
    Font      = fntUISemi,
    ForeColor = clrText
};
var lblCounter = new Label {
    Text      = "0 selected",
    Location  = new Point(12, 112),
    AutoSize  = true,
    Font      = fntUI,
    ForeColor = clrTextSec
};

// Tree view with checkboxes
var tree = new TreeView {
    Location    = new Point(12, 132),
    Size        = new Size(460, 360),
    CheckBoxes  = true,
    ShowLines   = false,
    BorderStyle = BorderStyle.FixedSingle,
    BackColor   = Color.White,
    Font        = fntUI,
    Indent      = 20,
    HotTracking = true
};

// ── Advanced Options group ─────────────────────────────────────────────────
var grpAdvanced = new GroupBox {
    Text      = "Advanced Options",
    Location  = new Point(12, 502),
    Size      = new Size(460, 58),
    Font      = fntUI,
    ForeColor = clrTextSec
};

var chkDimParam = new CheckBox {
    Text      = "Dimension Parameter \u2013 auto-select visible columns from one-side tables + add Table Origin column",
    Location  = new Point(8, 20),
    Size      = new Size(444, 30),
    AutoSize  = false,
    Font      = fntUI,
    ForeColor = clrText
};
grpAdvanced.Controls.Add(chkDimParam);

// ── Helpers ────────────────────────────────────────────────────────────────
// Save visible checked states back into the map before a rebuild or close.
Action saveState = () => {
    foreach (TreeNode tableNode in tree.Nodes)
        foreach (TreeNode child in tableNode.Nodes)
            if (child.Tag is ITabularTableObject obj)
                checkedState[obj] = child.Checked;
};

// Recompute and right-align the counter label.
Action updateCounter = () => {
    int n = checkedState.Count(kv => kv.Value);
    lblCounter.Text = n + " selected";
    lblCounter.Location = new Point(
        12 + 460 - TextRenderer.MeasureText(lblCounter.Text, lblCounter.Font).Width,
        112);
};

// Rebuild tree from allItems filtered by a search string.
// Callers must call saveState() themselves before modifying checkedState.
bool suppressCascade = false;
bool suppressEvents  = false;
Action<string> rebuildTree = filter => {
    suppressEvents = true;
    tree.BeginUpdate();
    tree.Nodes.Clear();

    var groups = allItems
        .Where(x => string.IsNullOrEmpty(filter) ||
                    x.TableName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
        .GroupBy(x => x.TableName);

    foreach (var group in groups)
    {
        var tableNode = new TreeNode(group.Key) { Tag = "table" };
        foreach (var item in group)
            tableNode.Nodes.Add(new TreeNode(item.DisplayName) {
                Tag     = item.Obj,
                Checked = checkedState[item.Obj]
            });
        int childCount   = tableNode.Nodes.Count;
        int checkedCount = tableNode.Nodes.Cast<TreeNode>().Count(n => n.Checked);
        bool allChecked  = childCount > 0 && checkedCount == childCount;
        bool someChecked = checkedCount > 0 && !allChecked;
        tableNode.Checked = allChecked;   // node not yet in tree — AfterCheck won't fire
        if (someChecked) {
            tableNode.Text      = group.Key + " (" + checkedCount + "/" + childCount + ")";
            tableNode.ForeColor = Color.SteelBlue;
        }
        bool expand = !string.IsNullOrEmpty(filter) || checkedCount > 0;
        if (expand) tableNode.Expand();
        tree.Nodes.Add(tableNode);
    }

    tree.EndUpdate();
    suppressEvents = false;
    updateCounter();
};

// Sync a table node’s checkbox and partial-selection badge after a child changes.
Action<TreeNode> updateTableNode = tableNode => {
    string tableName = tableNode.Text.Contains(" (")
        ? tableNode.Text.Substring(0, tableNode.Text.LastIndexOf(" ("))
        : tableNode.Text;
    int childCount   = tableNode.Nodes.Count;
    int checkedCount = tableNode.Nodes.Cast<TreeNode>().Count(n => n.Checked);
    bool allChecked  = childCount > 0 && checkedCount == childCount;
    bool someChecked = checkedCount > 0 && !allChecked;
    suppressEvents = true;
    tableNode.Checked = allChecked;
    suppressEvents = false;
    tableNode.Text      = someChecked ? tableName + " (" + checkedCount + "/" + childCount + ")" : tableName;
    tableNode.ForeColor = someChecked ? Color.SteelBlue : SystemColors.WindowText;
};

// ── Wire up events ────────────────────────────────────────────────────────
tree.AfterCheck += (s, e) => {
    if (suppressEvents || suppressCascade || e.Action == TreeViewAction.Unknown) return;
    if (e.Node.Tag as string == "table") {
        suppressCascade = true;
        foreach (TreeNode child in e.Node.Nodes) {
            child.Checked = e.Node.Checked;
            if (child.Tag is ITabularTableObject childObj)
                checkedState[childObj] = e.Node.Checked;
        }
        suppressCascade = false;
        updateTableNode(e.Node);
    } else if (e.Node.Tag is ITabularTableObject changed) {
        checkedState[changed] = e.Node.Checked;
        if (e.Node.Parent != null) updateTableNode(e.Node.Parent);
    }
    updateCounter();
};

txtSearch.TextChanged += (s, e) => { saveState(); rebuildTree(txtSearch.Text.Trim()); };

// When "Dimension Parameter" is toggled on, auto-check visible columns
// from tables that appear on the one-side of at least one relationship.
chkDimParam.CheckedChanged += (s, e) => {
    if (!chkDimParam.Checked) return;

    // Must save current tree state BEFORE modifying checkedState;
    // rebuildTree no longer calls saveState() itself.
    saveState();

    // Use name-based lookup (string) to avoid TOM reference-equality pitfalls.
    var dimTableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var rel in Model.Relationships) {
        if (rel.ToCardinality   == RelationshipEndCardinality.One) dimTableNames.Add(rel.ToTable.Name);
        if (rel.FromCardinality == RelationshipEndCardinality.One) dimTableNames.Add(rel.FromTable.Name);
    }

    // Check all visible (non-hidden) columns from dimension tables
    foreach (var item in allItems)
        if (item.Obj is Column col && !col.IsHidden && dimTableNames.Contains(col.Table.Name))
            checkedState[item.Obj] = true;

    rebuildTree(txtSearch.Text.Trim());  // saveState() already called above
};

// ── Initial population ────────────────────────────────────────────────────
rebuildTree(string.Empty);

// ── Buttons (page 1) ──────────────────────────────────────────────────────
var btnNext = new FluentButton {
    Text      = "Next \u2192",
    Width     = 100,
    IsPrimary = true,
    Location  = new Point(12 + 460 - 100, 570)
};
var btnCancel = new FluentButton {
    Text         = "Cancel",
    Width        = 80,
    DialogResult = DialogResult.Cancel
};
btnCancel.Location = new Point(btnNext.Left - btnCancel.Width - 8, 570);

form.Controls.AddRange(new Control[] {
    lblName, txtName,
    lblSearch, txtSearch,
    lblObjects, lblCounter,
    tree,
    grpAdvanced,
    btnNext, btnCancel
});
form.AcceptButton = btnNext;
form.CancelButton = btnCancel;

// ── Page 1: show selector ─────────────────────────────────────────────────
bool goNext = false;
btnNext.Click += (s, e) => { goNext = true; form.Close(); };

if (form.ShowDialog(dialogOwner) != DialogResult.OK && !goNext) return;
if (!goNext) return;

saveState();

// ── Build selection list in current tree order ────────────────────────────
var orderedSelection = allItems
    .Where(x => checkedState[x.Obj])
    .Select(x => (Obj: x.Obj, DisplayName: x.Obj.Name))
    .ToList();

if (orderedSelection.Count == 0)
    throw new Exception("No columns or measures selected.");

// ── Page 2: reorder & rename ──────────────────────────────────────────────
var form2 = new Form {
    Text            = "Review Selection \u2014 Reorder & Rename",
    Size            = new Size(640, 610),
    StartPosition   = FormStartPosition.CenterScreen,
    FormBorderStyle = FormBorderStyle.FixedDialog,
    MaximizeBox     = false,
    MinimizeBox     = false,
    BackColor       = Color.White,
    Font            = fntUI
};

var lbl2 = new Label {
    Text      = "Use \u25b2/\u25bc to reorder. Click Display Name or Table Origin to rename. Click \u00d7 to remove a row.",
    Location  = new Point(12, 14),
    AutoSize  = true,
    Font      = fntUI,
    ForeColor = clrTextSec
};

// DataGridView for the ordered list
var grid = new DataGridView {
    Location              = new Point(12, 40),
    Size                  = new Size(600, 400),
    AllowUserToAddRows    = false,
    AllowUserToDeleteRows = false,
    RowHeadersVisible     = true,
    RowHeadersWidth       = 24,
    SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
    MultiSelect           = false,
    AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
    BorderStyle           = BorderStyle.FixedSingle,
    CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal,
    GridColor             = Color.FromArgb(225, 223, 221),
    BackgroundColor       = Color.White,
    EnableHeadersVisualStyles = false
};
grid.RowTemplate.Height = 28;

// Fluent-style column headers
grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle {
    BackColor = Color.FromArgb(243, 242, 241),
    ForeColor = clrText,
    Font      = fntUISemi,
    Padding   = new Padding(4, 0, 0, 0)
};
grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

// Row cell styles
grid.DefaultCellStyle = new DataGridViewCellStyle {
    BackColor          = Color.White,
    ForeColor          = clrText,
    SelectionBackColor = Color.FromArgb(219, 235, 253),
    SelectionForeColor = clrText,
    Font               = fntUI,
    Padding            = new Padding(4, 0, 0, 0)
};
grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle {
    BackColor          = Color.FromArgb(250, 249, 248),
    SelectionBackColor = Color.FromArgb(219, 235, 253),
    SelectionForeColor = clrText
};
grid.RowHeadersDefaultCellStyle = new DataGridViewCellStyle {
    BackColor = Color.FromArgb(243, 242, 241)
};

var colTable   = new DataGridViewTextBoxColumn { HeaderText = "Table",        ReadOnly = true,  FillWeight = 22 };
var colField   = new DataGridViewTextBoxColumn { HeaderText = "Field",        ReadOnly = true,  FillWeight = 28 };
var colDisplay = new DataGridViewTextBoxColumn { HeaderText = "Display Name", ReadOnly = false, FillWeight = 30 };
var colOrigin  = new DataGridViewTextBoxColumn { HeaderText = "Table Origin", ReadOnly = false, FillWeight = 18, Visible = false };
var colRemove  = new DataGridViewButtonColumn  { HeaderText = "",             Width = 32, MinimumWidth = 32,
                                                 FillWeight = 1, Text = "\u00d7", UseColumnTextForButtonValue = true };
grid.Columns.AddRange(colTable, colField, colDisplay, colOrigin, colRemove);

foreach (var item in orderedSelection)
    grid.Rows.Add(item.Obj.Table.Name, item.Obj.Name, item.DisplayName, item.Obj.Table.Name, "");

// Style the remove button column cells
grid.CellPainting += (s, e) => {
    if (e.ColumnIndex != colRemove.Index || e.RowIndex < 0) return;
    e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
    using (var br = new SolidBrush(Color.FromArgb(196, 43, 28)))
        e.Graphics.DrawString("\u00d7", new Font("Segoe UI", 10f, FontStyle.Bold),
            br, e.CellBounds,
            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
    e.Handled = true;
};
grid.CellClick += (s, e) => {
    if (e.ColumnIndex == colRemove.Index && e.RowIndex >= 0)
        grid.Rows.RemoveAt(e.RowIndex);
};

// Table Origin toggle
var chkTableOrigin = new CheckBox {
    Text      = "Include Table Origin column (editable \u2014 used as hierarchy level)",
    Location  = new Point(12, 452),
    AutoSize  = true,
    Checked   = chkDimParam.Checked,
    Font      = fntUI,
    ForeColor = clrText
};
colOrigin.Visible = chkDimParam.Checked;
chkTableOrigin.CheckedChanged += (s, e) => colOrigin.Visible = chkTableOrigin.Checked;

// Move-up / move-down buttons
var btnUp = new FluentButton { Text = "\u25b2 Move Up",   Width = 100, Location = new Point(12,  488) };
var btnDn = new FluentButton { Text = "\u25bc Move Down", Width = 110, Location = new Point(118, 488) };

// Swap all editable cell values between two rows (excludes the button column)
Action<int, int> swapRows = (a, b) => {
    for (int c = 0; c < grid.Columns.Count - 1; c++) {
        var tmp = grid.Rows[a].Cells[c].Value;
        grid.Rows[a].Cells[c].Value = grid.Rows[b].Cells[c].Value;
        grid.Rows[b].Cells[c].Value = tmp;
    }
};
btnUp.Click += (s, e) => {
    if (grid.CurrentCell == null) return;
    int i = grid.CurrentCell.RowIndex;
    if (i <= 0) return;
    swapRows(i, i - 1);
    grid.ClearSelection();
    grid.CurrentCell = grid.Rows[i - 1].Cells[grid.CurrentCell.ColumnIndex];
    grid.Rows[i - 1].Selected = true;
};
btnDn.Click += (s, e) => {
    if (grid.CurrentCell == null) return;
    int i = grid.CurrentCell.RowIndex;
    if (i >= grid.Rows.Count - 1) return;
    swapRows(i, i + 1);
    grid.ClearSelection();
    grid.CurrentCell = grid.Rows[i + 1].Cells[grid.CurrentCell.ColumnIndex];
    grid.Rows[i + 1].Selected = true;
};

var btn2OK = new FluentButton {
    Text         = "Create Parameter",
    Width        = 145,
    IsPrimary    = true,
    DialogResult = DialogResult.OK,
    Location     = new Point(12 + 600 - 145, 488)
};
var btn2Cancel = new FluentButton {
    Text         = "Cancel",
    Width        = 80,
    DialogResult = DialogResult.Cancel,
    Location     = new Point(btn2OK.Left - 80 - 8, 488)
};
var btnBack = new FluentButton {
    Text     = "\u2190 Back",
    Width    = 80,
    Location = new Point(btn2Cancel.Left - 80 - 8, 488)
};

btnBack.Click += (s, e) => { form2.DialogResult = DialogResult.Retry; form2.Close(); };

form2.Controls.AddRange(new Control[] { lbl2, grid, chkTableOrigin, btnUp, btnDn, btnBack, btn2Cancel, btn2OK });
form2.AcceptButton = btn2OK;
form2.CancelButton = btn2Cancel;

var page2Result = form2.ShowDialog(dialogOwner);
if (page2Result == DialogResult.Retry) {
    // Back — loop back into page 1 by re-running from the top (script is stateless; simplest approach)
    // Re-show page 1 form with current state preserved
    goNext = false;
    form.ShowDialog(dialogOwner);
    if (!goNext) return;
    saveState();
    orderedSelection = allItems
        .Where(x => checkedState[x.Obj])
        .Select(x => (Obj: x.Obj, DisplayName: x.Obj.Name))
        .ToList();
    if (orderedSelection.Count == 0) throw new Exception("No columns or measures selected.");
    // Clear and repopulate grid
    grid.Rows.Clear();
    foreach (var item in orderedSelection)
        grid.Rows.Add(item.Obj.Table.Name, item.Obj.Name, item.DisplayName, item.Obj.Table.Name, "");
    if (form2.ShowDialog(dialogOwner) != DialogResult.OK) return;
} else if (page2Result != DialogResult.OK) {
    return;
}

// ── Read final ordered + renamed selection from the grid ──────────────────
var lookup = allItems.ToDictionary(x => x.TableName + "||" + x.Obj.Name, x => x.Obj);
var finalSelection = new List<(ITabularTableObject Obj, string Display)>();
foreach (DataGridViewRow row in grid.Rows) {
    string tbl  = row.Cells[0].Value?.ToString() ?? "";
    string fld  = row.Cells[1].Value?.ToString() ?? "";
    string disp = row.Cells[2].Value?.ToString() ?? fld;
    string key  = tbl + "||" + fld;
    ITabularTableObject obj;
    if (lookup.TryGetValue(key, out obj))
        finalSelection.Add((obj, disp));
}

// ── Validate parameter name (read from page 1 form) ───────────────────────
var paramName = txtName.Text.Trim();
if (string.IsNullOrEmpty(paramName))
    throw new Exception("Parameter table name cannot be empty.");

bool useTableOrigin = chkTableOrigin.Checked;

// ── Build the DAX for the calculated table ─────────────────────────────────
// When Table Origin is enabled the 4th tuple element uses the (possibly
// renamed) value from the grid's Table Origin cell for each row.
var daxRows = new List<string>();
for (int idx = 0; idx < finalSelection.Count; idx++) {
    var item = finalSelection[idx];
    var itemObj = item.Obj;
    var itemDisplay = item.Display;
    if (useTableOrigin) {
        string originVal = grid.Rows[idx].Cells[3].Value != null
            ? grid.Rows[idx].Cells[3].Value.ToString()
            : itemObj.Table.Name;
        daxRows.Add(string.Format("(\"{0}\", NAMEOF('{1}'[{2}]), {3}, \"{4}\")",
            itemDisplay, itemObj.Table.Name, itemObj.Name, idx, originVal));
    } else {
        daxRows.Add(string.Format("(\"{0}\", NAMEOF('{1}'[{2}]), {3})",
            itemDisplay, itemObj.Table.Name, itemObj.Name, idx));
    }
}
string dax = "{\n    " + string.Join(",\n    ", daxRows) + "\n}";

// ── Add the calculated table to the model ─────────────────────────────────
var paramTable = Model.AddCalculatedTable(paramName, dax);

// TE3 automatically creates columns from the DAX tuple; rename them to match convention.
var nameCol  = paramTable.Columns["Value1"] as CalculatedTableColumn;
var fieldCol = paramTable.Columns["Value2"] as CalculatedTableColumn;
var orderCol = paramTable.Columns["Value3"] as CalculatedTableColumn;

nameCol.IsNameInferred  = false;  nameCol.Name  = paramName;
fieldCol.IsNameInferred = false;  fieldCol.Name = paramName + " Fields";
orderCol.IsNameInferred = false;  orderCol.Name = paramName + " Order";

// ── Set field-parameter metadata ───────────────────────────────────────────
nameCol.SortByColumn  = orderCol;
nameCol.GroupByColumns.Add(fieldCol);
fieldCol.SortByColumn = orderCol;
fieldCol.SetExtendedProperty("ParameterMetadata", "{\"version\":3,\"kind\":2}", ExtendedPropertyType.Json);
fieldCol.IsHidden = true;
orderCol.IsHidden = true;

// ── Table Origin: add visible fourth column ───────────────────────────────
if (useTableOrigin) {
    var originCol = paramTable.Columns["Value4"] as CalculatedTableColumn;
    originCol.IsNameInferred = false;
    originCol.Name = paramName + " Table Origin";
    // Intentionally visible — use as hierarchy level in the field parameter
}

Info($"Field parameter table '{paramName}' created with {finalSelection.Count} field(s).{(useTableOrigin ? " (+ Table Origin)" : "")}");
