using System.Collections.Generic;

namespace NPMAPI.Models
{
    public class SpecialInstructionViewModel
    {
        //public List<SelectListViewModel> QuestionList { get; set; }
        public List<SelectListViewModel> CategoryList { get; set; }
        public List<Practice_Special_Instruction_Answers> specialInstructionModel { get; set; }
    }
}