using DiplomaManagement.Services;

namespace DiplomaManagement.Models
{
    public class PagerViewModel
    {
        public PagingService Pager { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
    }
}
