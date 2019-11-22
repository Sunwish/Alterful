using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Functions
{
    public struct InputAttribution
    {
        public string content;
        public int caretPosition;
        public bool selectEmpty;
        public int selectStart;
        public int selectEnd;
    }

    public static class ACompletion
    {
        /// <summary>
        /// 获取指令补全
        /// </summary>
        /// <param name="currentInput">当前的指令输入</param>
        /// <returns></returns>
        public static InputAttribution GetInstructionCompletion(InputAttribution currentInput)
        {
            if (string.IsNullOrEmpty(currentInput.content)) return currentInput;
            string targetInstructionLeft, targetInstructionPart, targetInstructionRight;
            GetInstructionCompletionDepart(currentInput, out targetInstructionLeft, out targetInstructionPart, out targetInstructionRight);
            if (string.IsNullOrEmpty(targetInstructionPart)) return currentInput;
            InputAttribution retn = currentInput; retn.selectEmpty = true;
            string cp = "";
            switch (GetInstructionCompletionPartType(targetInstructionPart))
            {
                case InstructionType.STARTUP:
                    cp = AInstruction_Startup.GetCompletion(targetInstructionPart);
                    break;
                case InstructionType.MACRO:
                    cp = AInstruction_Macro.GetCompletion(targetInstructionPart);
                    break;
                case InstructionType.CONST:
                    cp = AConstQuote.GetCompletion(targetInstructionPart);
                    break;
                case InstructionType.BUILDIN:
                    break;
            }

            if (!string.IsNullOrEmpty(cp))
            {
                retn.content = targetInstructionLeft + cp + targetInstructionRight;
                retn.selectStart = retn.caretPosition = currentInput.caretPosition;
                retn.selectEnd = retn.selectStart + cp.Length - targetInstructionPart.Length - 1;
                retn.selectEmpty = retn.selectEnd == 0;
            }
            return retn;
        }
        
        public static InstructionType GetInstructionCompletionPartType(string instructionPart)
        {
            if (string.IsNullOrEmpty(instructionPart)) return InstructionType.STARTUP;
            if (AInstruction.SYMBOL_CONST == instructionPart[0]) return InstructionType.CONST;
            return AInstruction.GetType(instructionPart);
        }

        public static void GetInstructionCompletionDepart(InputAttribution input, out string targetLeft, out string target, out string targetRight)
        {
            int blankBias = 1;
            int targetInstructionStart = input.content.Substring(0, input.caretPosition).IndexOf(" ");
            if (targetInstructionStart == -1) targetInstructionStart = blankBias = 0;
            target = input.content.Substring(targetInstructionStart + blankBias, input.caretPosition - targetInstructionStart - blankBias);
            targetLeft = input.content.Substring(0, targetInstructionStart + blankBias);
            targetRight = input.content.Substring(input.caretPosition) ?? "";
        }
    }
}
