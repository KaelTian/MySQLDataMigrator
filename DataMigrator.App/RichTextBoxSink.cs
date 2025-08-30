using Serilog.Core;
using Serilog.Events;

namespace DataMigrator.App
{
    // ========================================
    // 辅助类：Serilog RichTextBox Sink（将日志输出到WinForm文本框）
    // ========================================
    public class RichTextBoxSink : ILogEventSink
    {
        private readonly RichTextBox _richTextBox;

        public RichTextBoxSink(RichTextBox textBox)
        {
            _richTextBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
        }

        public void Emit(LogEvent logEvent)
        {
            // 注意：Serilog默认异步写入，需切换到UI线程更新文本框
            if (_richTextBox.InvokeRequired)
            {
                _richTextBox.Invoke(new Action<LogEvent>(Emit), logEvent);
                return;
            }

            // 格式化日志内容（仅显示Info及以上级别，避免Verbose/Debug刷屏）
            if (logEvent.Level < LogEventLevel.Information)
                return;

            var logMessage = logEvent.RenderMessage();
            var levelColor = logEvent.Level switch
            {
                LogEventLevel.Warning => Color.Orange,
                LogEventLevel.Error or LogEventLevel.Fatal => Color.Red,
                _ => _richTextBox.ForeColor // Info/Debug默认颜色
            };

            // 追加日志到文本框（保持最新日志在底部）
            _richTextBox.SelectionStart = _richTextBox.TextLength;
            _richTextBox.SelectionColor = levelColor;
            _richTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {logMessage}{Environment.NewLine}");

            // 自动滚动到底部
            _richTextBox.ScrollToCaret();
        }
    }
}
