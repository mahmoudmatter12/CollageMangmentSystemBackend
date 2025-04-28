namespace CollageMangmentSystem.Core.Entities.department
{
    public class Department
    {
        private static int StudentCount { get; set; }
        public Department()
        {
            StudentCount++;
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        // foreign key
        public string? HDDID { get; set; }
        public User? HDD { get; set; }


        public String GetDepartmentHDDName()
        {
            return HDD?.FullName ?? string.Empty;
        }

        public void AddNewStudent()
        {
            StudentCount++;
        }
        public void RemoveStudent()
        {
            if (StudentCount > 0)
            {
                StudentCount--;
            }
        }

        public int GetStudentCount()
        {
            return StudentCount;
        }

        public void setStudentCount(int count)
        {
            if (count >= 0)
            {
                StudentCount = count;
            }
        }

        // deconstructor
        // this deiconstructor will be called when the object is destroyed
        ~Department()
        {
            StudentCount--;
        }

    }
}