Place `SkillHub_Data.xlsx` in this folder. On startup the API (and the web app) load the
**Instructors**, **Participants** and **Workshops** sheets into the database. The **Enrollments**
sheet is only loaded through `POST /api/v1/enrollments/import`.
