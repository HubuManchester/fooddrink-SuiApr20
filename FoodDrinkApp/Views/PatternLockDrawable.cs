namespace FoodDrinkApp.Views;

/// <summary>
/// 九宫格手势解锁绘制器 —— 负责在 <see cref="GraphicsView"/> 上绘制 3×3 圆点矩阵、
/// 手势连线轨迹以及圆点的高亮/错误状态。
///
/// 设计说明：
/// - 九宫格索引 (0-8)，按行排列：0 1 2 / 3 4 5 / 6 7 8。
/// - 支持三种圆点状态：Normal（未选中）、Selected（已划过）、Error（错误反馈）。
/// - 连线按 VisitedIndices 的顺序绘制，当前拖拽位置用虚线拖尾表示。
/// - 所有尺寸基于 dirtyRect 动态计算，适配不同屏幕。
/// </summary>
public class PatternLockDrawable : IDrawable
{
    private const int GridSize = 3;
    private const float DotRadiusFraction = 0.042f;   // 圆点半径占画布宽度的比例
    private const float LineWidth = 6f;
    private const float TrailDashLength = 8f;

    /// <summary>已访问的圆点索引列表（按访问顺序）。</summary>
    public List<int> VisitedIndices { get; set; } = new();

    /// <summary>当前手指拖拽位置（用于绘制拖尾虚线）。</summary>
    public PointF? CurrentTouchPoint { get; set; }

    /// <summary>是否为错误状态（连线与圆点变红）。</summary>
    public bool IsErrorState { get; set; }

    // ── 主题色（可由外部覆盖） ──
    public Color NormalFillColor { get; set; } = Color.FromArgb("#FDE8ED");
    public Color NormalStrokeColor { get; set; } = Color.FromArgb("#D97080");
    public Color SelectedFillColor { get; set; } = Color.FromArgb("#D97080");
    public Color SelectedStrokeColor { get; set; } = Color.FromArgb("#C46070");
    public Color LineColor { get; set; } = Color.FromArgb("#D97080");
    public Color ErrorColor { get; set; } = Color.FromArgb("#E04040");

    /// <summary>
    /// 计算第 index 个圆点在画布上的中心坐标（索引 0-8，3×3 布局）。
    /// </summary>
    public static PointF GetDotCenter(int index, float canvasWidth, float canvasHeight)
    {
        float padding = canvasWidth * 0.15f;
        float usableWidth = canvasWidth - padding * 2;
        float usableHeight = canvasHeight - padding * 2;
        float cellWidth = usableWidth / (GridSize - 1);
        float cellHeight = usableHeight / (GridSize - 1);

        int row = index / GridSize;
        int col = index % GridSize;

        return new PointF(
            padding + col * cellWidth,
            padding + row * cellHeight
        );
    }

    /// <summary>
    /// 根据触摸坐标查找最近的圆点索引；若距离超过阈值则返回 null。
    /// </summary>
    public static int? HitTest(PointF touchPoint, float canvasWidth, float canvasHeight)
    {
        float radius = canvasWidth * DotRadiusFraction;
        float hitThreshold = radius * 2.2f; // 容错范围：略大于圆点本身

        for (int i = 0; i < GridSize * GridSize; i++)
        {
            var center = GetDotCenter(i, canvasWidth, canvasHeight);
            float dx = touchPoint.X - center.X;
            float dy = touchPoint.Y - center.Y;
            if (dx * dx + dy * dy <= hitThreshold * hitThreshold)
                return i;
        }

        return null;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;
        float radius = w * DotRadiusFraction;

        var lineColor = IsErrorState ? ErrorColor : LineColor;
        var selectedFill = IsErrorState ? ErrorColor : SelectedFillColor;

        // ── 第一步：绘制连线（在圆点下方）──
        if (VisitedIndices.Count > 1)
        {
            canvas.StrokeColor = lineColor;
            canvas.StrokeSize = LineWidth;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            for (int i = 1; i < VisitedIndices.Count; i++)
            {
                var from = GetDotCenter(VisitedIndices[i - 1], w, h);
                var to = GetDotCenter(VisitedIndices[i], w, h);
                canvas.DrawLine(from, to);
            }
        }

        // ── 第二步：拖尾虚线（当前手指位置 → 最后一个已访问圆点）──
        if (!IsErrorState && VisitedIndices.Count > 0 && CurrentTouchPoint.HasValue)
        {
            canvas.StrokeColor = lineColor.WithAlpha(0.55f);
            canvas.StrokeSize = LineWidth * 0.75f;
            canvas.StrokeDashPattern = new float[] { TrailDashLength, TrailDashLength };

            var last = GetDotCenter(VisitedIndices[^1], w, h);
            canvas.DrawLine(last, CurrentTouchPoint.Value);

            canvas.StrokeDashPattern = null; // 重置虚线
        }

        // ── 第三步：绘制圆点 ──
        var visitedSet = new HashSet<int>(VisitedIndices);

        for (int i = 0; i < GridSize * GridSize; i++)
        {
            var center = GetDotCenter(i, w, h);
            bool isSelected = visitedSet.Contains(i);

            // 外圈
            canvas.StrokeColor = isSelected ? (IsErrorState ? ErrorColor : SelectedStrokeColor) : NormalStrokeColor;
            canvas.StrokeSize = isSelected ? 3f : 2f;
            canvas.FillColor = isSelected ? selectedFill : NormalFillColor;
            canvas.FillCircle(center, radius);
            canvas.DrawCircle(center, radius);

            // 已选中的圆点内部再加一个小高亮圆
            if (isSelected)
            {
                canvas.FillColor = Colors.White.WithAlpha(0.55f);
                canvas.FillCircle(center, radius * 0.38f);
            }
        }
    }
}
