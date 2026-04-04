using System;
using System.Windows.Forms;

namespace SimpleCalculator
{
    public partial class Form1 : Form
    {
        // 정수 기반 연산 상태
        private int? operandA = null;
        private int? operandB = null;
        private string? pendingOperator = null;

        // 연산자 클릭 후 다음 숫자 입력에서 새 피연산자로 시작할지 여부
        private bool isNewEntry = false;

        public Form1()
        {
            InitializeComponent();

            // 숫자 버튼 이벤트 연결
            btnNum0.Click += NumButton_Click;
            btnNum1.Click += NumButton_Click;
            btnNum2.Click += NumButton_Click;
            btnNum3.Click += NumButton_Click;
            btnNum4.Click += NumButton_Click;
            btnNum5.Click += NumButton_Click;
            btnNum6.Click += NumButton_Click;
            btnNum7.Click += NumButton_Click;
            btnNum8.Click += NumButton_Click;
            btnNum9.Click += NumButton_Click;

            // 연산자 버튼 이벤트 연결 (= 포함)
            btnOpAdd.Click += OpButton_Click;
            btnOpSub.Click += OpButton_Click;
            btnOpMul.Click += OpButton_Click;
            btnOpDiv.Click += OpButton_Click;
            btnOpEqu.Click += OpButton_Click;
        }

        private void NumButton_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            string digit = btn.Text;

            // txtOutputWindow: isNewEntry가 true이면 새 피연산자로 덮어쓰기,
            // 그렇지 않으면 이어붙임
            if (isNewEntry || string.IsNullOrEmpty(txtOutputWindow.Text) || txtOutputWindow.Text == "0")
            {
                txtOutputWindow.Text = digit;
            }
            else
            {
                txtOutputWindow.Text += digit;
            }

            // txtInputWindow에는 연산자/공백 뒤에 숫자를 이어붙이는 방식 유지
            if (isNewEntry)
            {
                // 일반적으로 txtInputWindow는 "... op " 형태일 것임 -> 그냥 이어붙임
                txtInputWindow.Text += digit;
            }
            else
            {
                if (string.IsNullOrEmpty(txtInputWindow.Text) || txtInputWindow.Text == "0")
                    txtInputWindow.Text = digit;
                else
                    txtInputWindow.Text += digit;
            }

            // 숫자 입력이 시작되었으므로 새 입력 플래그 해제
            isNewEntry = false;
        }

        private void OpButton_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            // 표시용 기호 매핑 (txtInputWindow에 표시)
            string op;
            if (btn == btnOpAdd) op = "+";
            else if (btn == btnOpSub) op = "-";
            else if (btn == btnOpMul) op = "x";
            else if (btn == btnOpDiv) op = "÷";
            else if (btn == btnOpEqu) op = "=";
            else op = btn.Text;

            // txtInputWindow에 연산자 표시 (기존 동작 유지)
            string current = txtInputWindow.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(current))
            {
                txtInputWindow.Text = op;
            }
            else
            {
                string trimmed = current.TrimEnd();
                if (trimmed.Length > 0 && "+-x÷=".Contains(trimmed[^1]))
                {
                    string withoutLast = trimmed.Substring(0, trimmed.Length - 1).TrimEnd();
                    txtInputWindow.Text = string.IsNullOrEmpty(withoutLast) ? op : $"{withoutLast} {op} ";
                }
                else
                {
                    txtInputWindow.Text = $"{current} {op} ";
                }
            }

            // txtOutputWindow의 현재 텍스트를 정수로 파싱해서 operand에 저장
            string outText = (txtOutputWindow.Text ?? string.Empty).Trim();
            bool parsedNow = int.TryParse(outText, out int parsed);

            if (parsedNow)
            {
                if (!operandA.HasValue)
                    operandA = parsed;
                else if (!operandB.HasValue)
                    operandB = parsed;
            }

            // "=" 처리: 계산 수행
            if (op == "=")
            {
                // '=' 누르기 직전에 파싱된 값으로 B 채움(필요한 경우)
                if (parsedNow && !operandB.HasValue && operandA.HasValue && pendingOperator != null)
                    operandB = parsed;

                if (operandA.HasValue && operandB.HasValue && !string.IsNullOrEmpty(pendingOperator))
                {
                    try
                    {
                        int result = pendingOperator switch
                        {
                            "+" => operandA.Value + operandB.Value,
                            "-" => operandA.Value - operandB.Value,
                            "x" => operandA.Value * operandB.Value,
                            "÷" => operandB.Value == 0 ? throw new DivideByZeroException() : operandA.Value / operandB.Value,
                            _ => throw new InvalidOperationException("Unknown operator")
                        };

                        // 결과를 출력창에 표시
                        txtOutputWindow.Text = result.ToString();

                        // txtInputWindow에는 이미 "... = " 형태로 연산자가 붙어있으므로 결과를 뒤에 붙임
                        txtInputWindow.Text = txtInputWindow.Text + result.ToString();

                        // 연속 계산을 위해 operandA에 결과를 남기고 operandB/pendingOperator 초기화
                        operandA = result;
                        operandB = null;
                        pendingOperator = null;

                        // 결과가 표시된 상태이므로 다음 입력은 새 피연산자로 시작
                        isNewEntry = true;
                    }
                    catch (DivideByZeroException)
                    {
                        txtOutputWindow.Text = "Error";
                        txtInputWindow.Text = "Error";
                        operandA = null;
                        operandB = null;
                        pendingOperator = null;
                        isNewEntry = true;
                    }
                    catch
                    {
                        txtOutputWindow.Text = "Error";
                        txtInputWindow.Text = "Error";
                        operandA = null;
                        operandB = null;
                        pendingOperator = null;
                        isNewEntry = true;
                    }
                }
                // '=' 후에는 출력창을 초기화하지 않음(결과 유지)
            }
            else
            {
                // 연산자(= 제외) 처리:
                // - 내부 상태(operandA/pendingOperator/operandB)는 필요한 경우 업데이트(체이닝용)
                // - 그러나 txtOutputWindow는 연산자 클릭시 변화 없음(요청대로)
                if (operandA.HasValue && pendingOperator != null && operandB.HasValue)
                {
                    try
                    {
                        int interim = pendingOperator switch
                        {
                            "+" => operandA.Value + operandB.Value,
                            "-" => operandA.Value - operandB.Value,
                            "x" => operandA.Value * operandB.Value,
                            "÷" => operandB.Value == 0 ? throw new DivideByZeroException() : operandA.Value / operandB.Value,
                            _ => throw new InvalidOperationException("Unknown operator")
                        };

                        // 내부적으로 결과를 다음 연산의 시작값으로 사용
                        operandA = interim;
                        operandB = null;
                        // txtOutputWindow는 변경하지 않음(요청사항)
                    }
                    catch (DivideByZeroException)
                    {
                        txtOutputWindow.Text = "Error";
                        txtInputWindow.Text = "Error";
                        operandA = null;
                        operandB = null;
                        pendingOperator = null;
                        isNewEntry = true;
                        return;
                    }
                    catch
                    {
                        txtOutputWindow.Text = "Error";
                        txtInputWindow.Text = "Error";
                        operandA = null;
                        operandB = null;
                        pendingOperator = null;
                        isNewEntry = true;
                        return;
                    }
                }

                // 새로운 연산자 저장 및 다음 숫자 입력은 새 항목으로 시작하게 플래그 설정
                pendingOperator = op;
                isNewEntry = true;
            }
        }
    }
}
